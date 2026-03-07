using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Crypto;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Configuration.Events;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download.Aggregation;
using Playarr.Core.Indexers;
using Playarr.Core.Jobs;
using Playarr.Core.Lifecycle;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Queue;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision, PendingReleaseReason reason);
        void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions);
        List<ReleaseInfo> GetPending();
        List<RemoteEpisode> GetPendingRemoteEpisodes(int gameId);
        List<Queue.Queue> GetPendingQueue();
        Queue.Queue FindPendingQueueItem(int queueId);
        void RemovePendingQueueItems(int queueId);
        RemoteEpisode OldestPendingRelease(int gameId, int[] romIds);
        List<Queue.Queue> GetPendingQueueObsolete();
        Queue.Queue FindPendingQueueItemObsolete(int queueId);
        void RemovePendingQueueItemsObsolete(int queueId);
    }

    public class PendingReleaseService : IPendingReleaseService,
                                         IHandle<SeriesEditedEvent>,
                                         IHandle<SeriesUpdatedEvent>,
                                         IHandle<SeriesDeletedEvent>,
                                         IHandle<EpisodeGrabbedEvent>,
                                         IHandle<RssSyncCompleteEvent>,
                                         IHandle<QualityProfileUpdatedEvent>,
                                         IHandle<ConfigSavedEvent>,
                                         IHandle<ApplicationStartedEvent>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IPendingReleaseRepository _repository;
        private readonly IGameService _seriesService;
        private readonly IParsingService _parsingService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly ITaskManager _taskManager;
        private readonly IConfigService _configService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IRemoteEpisodeAggregationService _aggregationService;
        private readonly IDownloadClientFactory _downloadClientFactory;
        private readonly IIndexerFactory _indexerFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private static List<PendingRelease> _pendingReleases = new();

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    IGameService seriesService,
                                    IParsingService parsingService,
                                    IDelayProfileService delayProfileService,
                                    ITaskManager taskManager,
                                    IConfigService configService,
                                    ICustomFormatCalculationService formatCalculator,
                                    IRemoteEpisodeAggregationService aggregationService,
                                    IDownloadClientFactory downloadClientFactory,
                                    IIndexerFactory indexerFactory,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _repository = repository;
            _seriesService = seriesService;
            _parsingService = parsingService;
            _delayProfileService = delayProfileService;
            _taskManager = taskManager;
            _configService = configService;
            _formatCalculator = formatCalculator;
            _aggregationService = aggregationService;
            _downloadClientFactory = downloadClientFactory;
            _indexerFactory = indexerFactory;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Add(DownloadDecision decision, PendingReleaseReason reason)
        {
            AddMany(new List<Tuple<DownloadDecision, PendingReleaseReason>> { Tuple.Create(decision, reason) });
        }

        public void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions)
        {
            var pending = _pendingReleases;

            foreach (var seriesDecisions in decisions.GroupBy(v => v.Item1.RemoteEpisode.Game.Id))
            {
                var game = seriesDecisions.First().Item1.RemoteEpisode.Game;
                var alreadyPending = _pendingReleases.Where(p => p.SeriesId == game.Id).SelectList(s => s.JsonClone());

                // TODO: Do we need IncludeRemoteEpisodes?
                alreadyPending = IncludeRemoteEpisodes(alreadyPending, seriesDecisions.ToDictionaryIgnoreDuplicates(v => v.Item1.RemoteEpisode.Release.Title, v => v.Item1.RemoteEpisode));
                var alreadyPendingByEpisode = CreateEpisodeLookup(alreadyPending);

                foreach (var pair in seriesDecisions)
                {
                    var decision = pair.Item1;
                    var reason = pair.Item2;

                    var romIds = decision.RemoteEpisode.Roms.Select(e => e.Id);

                    var existingReports = romIds.SelectMany(v => alreadyPendingByEpisode[v])
                                                    .Distinct().ToList();

                    var matchingReports = existingReports.Where(MatchingReleasePredicate(decision.RemoteEpisode.Release)).ToList();

                    if (matchingReports.Any())
                    {
                        var matchingReport = matchingReports.First();

                        if (matchingReport.Reason != reason)
                        {
                            if (matchingReport.Reason == PendingReleaseReason.DownloadClientUnavailable)
                            {
                                _logger.Debug("The release {0} is already pending with reason {1}, not changing reason", decision.RemoteEpisode, matchingReport.Reason);
                            }
                            else
                            {
                                _logger.Debug("The release {0} is already pending with reason {1}, changing to {2}", decision.RemoteEpisode, matchingReport.Reason, reason);
                                matchingReport.Reason = reason;
                                _repository.Update(matchingReport);
                            }
                        }
                        else
                        {
                            _logger.Debug("The release {0} is already pending with reason {1}, not adding again", decision.RemoteEpisode, reason);
                        }

                        if (matchingReports.Count > 1)
                        {
                            _logger.Debug("The release {0} had {1} duplicate pending, removing duplicates.", decision.RemoteEpisode, matchingReports.Count - 1);

                            foreach (var duplicate in matchingReports.Skip(1))
                            {
                                _repository.Delete(duplicate.Id);
                                alreadyPending.Remove(duplicate);
                                alreadyPendingByEpisode = CreateEpisodeLookup(alreadyPending);
                            }
                        }

                        continue;
                    }

                    _logger.Debug("Adding release {0} to pending releases with reason {1}", decision.RemoteEpisode, reason);
                    Insert(decision, reason);
                }
            }

            UpdatePendingReleases();
        }

        public List<ReleaseInfo> GetPending()
        {
            var releases = _repository.All().Select(p =>
            {
                var release = p.Release;

                release.PendingReleaseReason = p.Reason;

                return release;
            }).ToList();

            if (releases.Any())
            {
                releases = FilterBlockedIndexers(releases);
            }

            return releases;
        }

        public List<RemoteEpisode> GetPendingRemoteEpisodes(int gameId)
        {
            return _pendingReleases.Where(p => p.SeriesId == gameId).Select(p => p.RemoteEpisode).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();
            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));
            var pendingReleases = _pendingReleases.Where(p => p.Reason != PendingReleaseReason.Fallback).ToList();

            foreach (var pendingRelease in pendingReleases)
            {
                if (pendingRelease.RemoteEpisode.Roms.Empty())
                {
                    var noEpisodeItem = GetQueueItem(pendingRelease, nextRssSync, []);

                    noEpisodeItem.ErrorMessage = "Unable to find matching rom(s)";

                    queued.Add(noEpisodeItem);

                    continue;
                }

                queued.Add(GetQueueItem(pendingRelease, nextRssSync, pendingRelease.RemoteEpisode.Roms));
            }

            // Return best quality release for each rom group, this may result in multiple for the same rom if the roms in each release differ
            var deduped = queued.Where(q => q.Roms.Any()).GroupBy(q => q.Roms.Select(e => e.Id)).Select(g =>
            {
                var game = g.First().Game;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(game.QualityProfile))
                        .ThenBy(q => PrioritizeDownloadProtocol(q.Game, q.Protocol))
                        .First();
            });

            return deduped.ToList();
        }

        public List<Queue.Queue> GetPendingQueueObsolete()
        {
            var queued = new List<Queue.Queue>();
            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));
            var pendingReleases = _pendingReleases.Where(p => p.Reason != PendingReleaseReason.Fallback).ToList();

            foreach (var pendingRelease in pendingReleases)
            {
                if (pendingRelease.RemoteEpisode.Roms.Empty())
                {
                    var noEpisodeItem = GetQueueItem(pendingRelease, nextRssSync, (Rom)null);

                    noEpisodeItem.ErrorMessage = "Unable to find matching rom(s)";

                    queued.Add(noEpisodeItem);

                    continue;
                }

                foreach (var rom in pendingRelease.RemoteEpisode.Roms)
                {
                    queued.Add(GetQueueItem(pendingRelease, nextRssSync, rom));
                }
            }

#pragma warning disable CS0612

            // Return best quality release for each rom
            var deduped = queued.Where(q => q.Rom != null).GroupBy(q => q.Rom.Id).Select(g =>
            {
                var game = g.First().Game;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(game.QualityProfile))
                    .ThenBy(q => PrioritizeDownloadProtocol(q.Game, q.Protocol))
                    .First();
            });
#pragma warning restore CS0612

            return deduped.ToList();
        }

        public Queue.Queue FindPendingQueueItem(int queueId)
        {
            return GetPendingQueue().SingleOrDefault(p => p.Id == queueId);
        }

        public Queue.Queue FindPendingQueueItemObsolete(int queueId)
        {
            return GetPendingQueue().SingleOrDefault(p => p.Id == queueId);
        }

        public void RemovePendingQueueItems(int queueId)
        {
            var targetItem = FindPendingRelease(queueId);
            var seriesReleases = _repository.AllByGameId(targetItem.SeriesId);

            var releasesToRemove = seriesReleases.Where(
                c => c.ParsedRomInfo.SeasonNumber == targetItem.ParsedRomInfo.SeasonNumber &&
                     c.ParsedRomInfo.RomNumbers.SequenceEqual(targetItem.ParsedRomInfo.RomNumbers));

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public void RemovePendingQueueItemsObsolete(int queueId)
        {
            var targetItem = FindPendingReleaseObsolete(queueId);
            var seriesReleases = _repository.AllByGameId(targetItem.SeriesId);

            var releasesToRemove = seriesReleases.Where(
                c => c.ParsedRomInfo.SeasonNumber == targetItem.ParsedRomInfo.SeasonNumber &&
                     c.ParsedRomInfo.RomNumbers.SequenceEqual(targetItem.ParsedRomInfo.RomNumbers));

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public RemoteEpisode OldestPendingRelease(int gameId, int[] romIds)
        {
            var seriesReleases = GetPendingReleases(gameId);

            return seriesReleases.Select(r => r.RemoteEpisode)
                                 .Where(r => r.Roms.Select(e => e.Id).Intersect(romIds).Any())
                                 .MaxBy(p => p.Release.AgeHours);
        }

        private ILookup<int, PendingRelease> CreateEpisodeLookup(IEnumerable<PendingRelease> alreadyPending)
        {
            return alreadyPending.SelectMany(v => v.RemoteEpisode.Roms
                                                   .Select(d => new { Rom = d, PendingRelease = v }))
                                 .ToLookup(v => v.Rom.Id, v => v.PendingRelease);
        }

        private List<ReleaseInfo> FilterBlockedIndexers(List<ReleaseInfo> releases)
        {
            var blockedIndexers = new HashSet<int>(_indexerStatusService.GetBlockedProviders().Select(v => v.ProviderId));

            return releases.Where(release => !blockedIndexers.Contains(release.IndexerId)).ToList();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            return _pendingReleases;
        }

        private List<PendingRelease> GetPendingReleases(int gameId)
        {
            return _pendingReleases.Where(p => p.SeriesId == gameId).ToList();
        }

        private List<PendingRelease> IncludeRemoteEpisodes(List<PendingRelease> releases, Dictionary<string, RemoteEpisode> knownRemoteEpisodes = null)
        {
            var result = new List<PendingRelease>();

            var seriesMap = new Dictionary<int, Game>();

            if (knownRemoteEpisodes != null)
            {
                foreach (var game in knownRemoteEpisodes.Values.Select(v => v.Game))
                {
                    seriesMap.TryAdd(game.Id, game);
                }
            }

            foreach (var game in _seriesService.GetSeries(releases.Select(v => v.SeriesId).Distinct().Where(v => !seriesMap.ContainsKey(v))))
            {
                seriesMap[game.Id] = game;
            }

            foreach (var release in releases)
            {
                var game = seriesMap.GetValueOrDefault(release.SeriesId);

                // Just in case the game was removed, but wasn't cleaned up yet (housekeeper will clean it up)
                if (game == null)
                {
                    continue;
                }

                // Languages will be empty if added before upgrading to v4, reparsing the languages if they're empty will set it to Unknown or better.
                if (release.ParsedRomInfo.Languages.Empty())
                {
                    release.ParsedRomInfo.Languages = LanguageParser.ParseLanguages(release.Title);
                }

                release.RemoteEpisode = new RemoteEpisode
                {
                    Game = game,
                    SeriesMatchType = release.AdditionalInfo?.SeriesMatchType ?? SeriesMatchType.Unknown,
                    ReleaseSource = release.AdditionalInfo?.ReleaseSource ?? ReleaseSourceType.Unknown,
                    ParsedRomInfo = release.ParsedRomInfo,
                    Release = release.Release
                };

                if (knownRemoteEpisodes != null && knownRemoteEpisodes.TryGetValue(release.Release.Title, out var knownRemoteEpisode))
                {
                    release.RemoteEpisode.MappedPlatformNumber = knownRemoteEpisode.MappedPlatformNumber;
                    release.RemoteEpisode.Roms = knownRemoteEpisode.Roms;
                }
                else if (ValidateParsedRomInfo.ValidateForGameType(release.ParsedRomInfo, game))
                {
                    try
                    {
                        var remoteRom = _parsingService.Map(release.ParsedRomInfo, game);

                        release.RemoteEpisode.MappedPlatformNumber = remoteRom.MappedPlatformNumber;
                        release.RemoteEpisode.Roms = remoteRom.Roms;
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.Debug(ex, ex.Message);

                        release.RemoteEpisode.MappedPlatformNumber = release.ParsedRomInfo.SeasonNumber;
                        release.RemoteEpisode.Roms = new List<Rom>();
                    }
                }
                else
                {
                    release.RemoteEpisode.MappedPlatformNumber = release.ParsedRomInfo.SeasonNumber;
                    release.RemoteEpisode.Roms = new List<Rom>();
                }

                _aggregationService.Augment(release.RemoteEpisode);
                release.RemoteEpisode.CustomFormats = _formatCalculator.ParseCustomFormat(release.RemoteEpisode, release.Release.Size);

                result.Add(release);
            }

            return result;
        }

        private Queue.Queue GetQueueItem(PendingRelease pendingRelease, Lazy<DateTime> nextRssSync, List<Rom> roms)
        {
            var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteEpisode));

            if (ect < nextRssSync.Value)
            {
                ect = nextRssSync.Value;
            }
            else
            {
                ect = ect.AddMinutes(_configService.RssSyncInterval);
            }

            var timeLeft = ect.Subtract(DateTime.UtcNow);

            if (timeLeft.TotalSeconds < 0)
            {
                timeLeft = TimeSpan.Zero;
            }

            string downloadClientName = null;
            var indexer = _indexerFactory.Find(pendingRelease.Release.IndexerId);

            if (indexer is { DownloadClientId: > 0 })
            {
                var downloadClient = _downloadClientFactory.Find(indexer.DownloadClientId);

                downloadClientName = downloadClient?.Name;
            }

            var queue = new Queue.Queue
            {
                Id = GetQueueId(pendingRelease),
                Game = pendingRelease.RemoteEpisode.Game,
                Roms = roms,
                Languages = pendingRelease.RemoteEpisode.Languages,
                Quality = pendingRelease.RemoteEpisode.ParsedRomInfo.Quality,
                Title = pendingRelease.Title,
                Size = pendingRelease.RemoteEpisode.Release.Size,
                SizeLeft = pendingRelease.RemoteEpisode.Release.Size,
                RemoteEpisode = pendingRelease.RemoteEpisode,
                TimeLeft = timeLeft,
                EstimatedCompletionTime = ect,
                Added = pendingRelease.Added,
                Status = Enum.TryParse(pendingRelease.Reason.ToString(), out QueueStatus outValue) ? outValue : QueueStatus.Unknown,
                Protocol = pendingRelease.RemoteEpisode.Release.DownloadProtocol,
                Indexer = pendingRelease.RemoteEpisode.Release.Indexer,
                DownloadClient = downloadClientName
            };

            return queue;
        }

        private Queue.Queue GetQueueItem(PendingRelease pendingRelease, Lazy<DateTime> nextRssSync, Rom rom)
        {
            var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteEpisode));

            if (ect < nextRssSync.Value)
            {
                ect = nextRssSync.Value;
            }
            else
            {
                ect = ect.AddMinutes(_configService.RssSyncInterval);
            }

            var timeLeft = ect.Subtract(DateTime.UtcNow);

            if (timeLeft.TotalSeconds < 0)
            {
                timeLeft = TimeSpan.Zero;
            }

            string downloadClientName = null;
            var indexer = _indexerFactory.Find(pendingRelease.Release.IndexerId);

            if (indexer is { DownloadClientId: > 0 })
            {
                var downloadClient = _downloadClientFactory.Find(indexer.DownloadClientId);

                downloadClientName = downloadClient?.Name;
            }

            var queue = new Queue.Queue
            {
                Id = GetQueueId(pendingRelease, rom),
                Game = pendingRelease.RemoteEpisode.Game,

#pragma warning disable CS0612
                Rom = rom,
#pragma warning restore CS0612

                Languages = pendingRelease.RemoteEpisode.Languages,
                Quality = pendingRelease.RemoteEpisode.ParsedRomInfo.Quality,
                Title = pendingRelease.Title,
                Size = pendingRelease.RemoteEpisode.Release.Size,
                SizeLeft = pendingRelease.RemoteEpisode.Release.Size,
                RemoteEpisode = pendingRelease.RemoteEpisode,
                TimeLeft = timeLeft,
                EstimatedCompletionTime = ect,
                Added = pendingRelease.Added,
                Status = Enum.TryParse(pendingRelease.Reason.ToString(), out QueueStatus outValue) ? outValue : QueueStatus.Unknown,
                Protocol = pendingRelease.RemoteEpisode.Release.DownloadProtocol,
                Indexer = pendingRelease.RemoteEpisode.Release.Indexer,
                DownloadClient = downloadClientName
            };

            return queue;
        }

        private void Insert(DownloadDecision decision, PendingReleaseReason reason)
        {
            _repository.Insert(new PendingRelease
            {
                SeriesId = decision.RemoteEpisode.Game.Id,
                ParsedRomInfo = decision.RemoteEpisode.ParsedRomInfo,
                Release = decision.RemoteEpisode.Release,
                Title = decision.RemoteEpisode.Release.Title,
                Added = DateTime.UtcNow,
                Reason = reason,
                AdditionalInfo = new PendingReleaseAdditionalInfo
                {
                    SeriesMatchType = decision.RemoteEpisode.SeriesMatchType,
                    ReleaseSource = decision.RemoteEpisode.ReleaseSource
                }
            });

            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private void Delete(PendingRelease pendingRelease)
        {
            _repository.Delete(pendingRelease);
            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private int GetDelay(RemoteEpisode remoteRom)
        {
            var delayProfile = _delayProfileService.AllForTags(remoteRom.Game.Tags).OrderBy(d => d.Order).First();
            var delay = delayProfile.GetProtocolDelay(remoteRom.Release.DownloadProtocol);
            var minimumAge = _configService.MinimumAge;

            return new[] { delay, minimumAge }.Max();
        }

        private void RemoveGrabbed(RemoteEpisode remoteRom)
        {
            var pendingReleases = GetPendingReleases(remoteRom.Game.Id);
            var romIds = remoteRom.Roms.Select(e => e.Id);

            var existingReports = pendingReleases.Where(r => r.RemoteEpisode.Roms.Select(e => e.Id)
                                                             .Intersect(romIds)
                                                             .Any())
                                                             .ToList();

            if (existingReports.Empty())
            {
                return;
            }

            var profile = remoteRom.Game.QualityProfile;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteRom.ParsedRomInfo.Quality,
                                                                        existingReport.RemoteEpisode.ParsedRomInfo.Quality);

                // Only remove lower/equal quality pending releases
                // It is safer to retry these releases on the next round than remove it and try to re-add it (if its still in the feed)
                if (compare >= 0)
                {
                    _logger.Debug("Removing previously pending release, as it was grabbed.");
                    Delete(existingReport);
                }
            }
        }

        private void RemoveRejected(List<DownloadDecision> rejected)
        {
            _logger.Debug("Removing failed releases from pending");
            var pending = GetPendingReleases();

            foreach (var rejectedRelease in rejected)
            {
                var matching = pending.Where(MatchingReleasePredicate(rejectedRelease.RemoteEpisode.Release));

                foreach (var pendingRelease in matching)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(pendingRelease);
                }
            }
        }

        private PendingRelease FindPendingRelease(int queueId)
        {
            return GetPendingReleases().First(p => GetQueueId(p) == queueId);
        }

        private PendingRelease FindPendingReleaseObsolete(int queueId)
        {
            return GetPendingReleases().First(p => p.RemoteEpisode.Roms.Any(e => queueId == GetQueueId(p, e)));
        }

        private int GetQueueId(PendingRelease pendingRelease)
        {
            return HashConverter.GetHashInt31(string.Format("pending-{0}", pendingRelease.Id));
        }

        private int GetQueueId(PendingRelease pendingRelease, Rom rom)
        {
            return HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", pendingRelease.Id, rom?.Id ?? 0));
        }

        private int PrioritizeDownloadProtocol(Game game, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(game.Tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }

        private void UpdatePendingReleases()
        {
            _pendingReleases = IncludeRemoteEpisodes(_repository.All().ToList());
        }

        public void Handle(SeriesEditedEvent message)
        {
            UpdatePendingReleases();
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            UpdatePendingReleases();
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _repository.DeleteByGameIds(message.Game.Select(m => m.Id).ToList());
            UpdatePendingReleases();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            RemoveGrabbed(message.Rom);
            UpdatePendingReleases();
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
            UpdatePendingReleases();
        }

        public void Handle(QualityProfileUpdatedEvent message)
        {
            UpdatePendingReleases();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            UpdatePendingReleases();
        }

        public void Handle(ConfigSavedEvent message)
        {
            UpdatePendingReleases();
        }

        private static Func<PendingRelease, bool> MatchingReleasePredicate(ReleaseInfo release)
        {
            return p => p.Title == release.Title &&
                        p.Release.PublishDate == release.PublishDate &&
                        p.Release.Indexer == release.Indexer;
        }
    }
}
