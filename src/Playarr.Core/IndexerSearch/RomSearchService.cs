using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Queue;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch
{
    public class EpisodeSearchService : IExecute<EpisodeSearchCommand>,
                                        IExecute<MissingEpisodeSearchCommand>,
                                        IExecute<CutoffUnmetEpisodeSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IRomService _episodeService;
        private readonly IEpisodeCutoffService _episodeCutoffService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public EpisodeSearchService(ISearchForReleases releaseSearchService,
                                    IProcessDownloadDecisions processDownloadDecisions,
                                    IRomService episodeService,
                                    IEpisodeCutoffService episodeCutoffService,
                                    IQueueService queueService,
                                    Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeService = episodeService;
            _episodeCutoffService = episodeCutoffService;
            _queueService = queueService;
            _logger = logger;
        }

        private async Task SearchForBulkEpisodes(List<Rom> roms, bool monitoredOnly, bool userInvokedSearch)
        {
            _logger.ProgressInfo("Performing search for {0} roms", roms.Count);
            var downloadedCount = 0;
            var groups = new List<EpisodeSearchGroup>();

            foreach (var game in roms.GroupBy(e => e.SeriesId))
            {
                foreach (var platform in game.Select(e => e).GroupBy(e => e.SeasonNumber))
                {
                    groups.Add(new EpisodeSearchGroup
                    {
                        SeriesId = game.Key,
                        SeasonNumber = platform.Key,
                        Roms = platform.ToList()
                    });
                }
            }

            foreach (var group in groups.OrderBy(g => g.Roms.Min(e => e.LastSearchTime ?? DateTime.MinValue)))
            {
                List<DownloadDecision> decisions;

                var gameId = group.SeriesId;
                var platformNumber = group.SeasonNumber;
                var groupEpisodes = group.Roms;

                if (groupEpisodes.Count > 1)
                {
                    try
                    {
                        decisions = await _releaseSearchService.SeasonSearch(gameId, platformNumber, groupEpisodes, monitoredOnly, userInvokedSearch, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Unable to search for roms in platform {0} of [{1}]", platformNumber, gameId);
                        continue;
                    }
                }
                else
                {
                    var rom = groupEpisodes.First();

                    try
                    {
                        decisions = await _releaseSearchService.EpisodeSearch(rom, userInvokedSearch, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Unable to search for rom: [{0}]", rom);
                        continue;
                    }
                }

                var processed = await _processDownloadDecisions.ProcessDecisions(decisions);

                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed search for {0} roms. {1} reports downloaded.", roms.Count, downloadedCount);
        }

        private bool IsMonitored(bool episodeMonitored, bool seriesMonitored)
        {
            return episodeMonitored && seriesMonitored;
        }

        public void Execute(EpisodeSearchCommand message)
        {
            foreach (var romId in message.RomIds)
            {
                var decisions = _releaseSearchService.EpisodeSearch(romId, message.Trigger == CommandTrigger.Manual, false).GetAwaiter().GetResult();
                var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

                _logger.ProgressInfo("Rom search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }

        public void Execute(MissingEpisodeSearchCommand message)
        {
            var monitored = message.Monitored;
            List<Rom> roms;

            if (message.SeriesId.HasValue)
            {
                roms = _episodeService.GetEpisodeBySeries(message.SeriesId.Value)
                                          .Where(e => e.Monitored == monitored &&
                                                 !e.HasFile &&
                                                 e.AirDateUtc.HasValue &&
                                                 e.AirDateUtc.Value.Before(DateTime.UtcNow))
                                          .ToList();
            }
            else
            {
                var pagingSpec = new PagingSpec<Rom>
                                 {
                                     Page = 1,
                                     PageSize = 1000000,
                                     SortDirection = SortDirection.Ascending,
                                     SortKey = "Id"
                                 };

                if (monitored)
                {
                    pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Game.Monitored == true);
                }
                else
                {
                    pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Game.Monitored == false);
                }

                roms = _episodeService.EpisodesWithoutFiles(pagingSpec).Records.ToList();
            }

            var queue = GetQueuedRomIds();
            var missing = roms.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForBulkEpisodes(missing, monitored, message.Trigger == CommandTrigger.Manual).GetAwaiter().GetResult();
        }

        public void Execute(CutoffUnmetEpisodeSearchCommand message)
        {
            var monitored = message.Monitored;

            var pagingSpec = new PagingSpec<Rom>
                             {
                                 Page = 1,
                                 PageSize = 100000,
                                 SortDirection = SortDirection.Ascending,
                                 SortKey = "Id"
                             };

            if (message.SeriesId.HasValue)
            {
                var gameId = message.SeriesId.Value;
                pagingSpec.FilterExpressions.Add(v => v.SeriesId == gameId);
            }

            if (monitored)
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Game.Monitored == true);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Game.Monitored == false);
            }

            var roms = _episodeCutoffService.EpisodesWhereCutoffUnmet(pagingSpec).Records.ToList();
            var queue = GetQueuedRomIds();
            var cutoffUnmet = roms.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForBulkEpisodes(cutoffUnmet, monitored, message.Trigger == CommandTrigger.Manual).GetAwaiter().GetResult();
        }

        private List<int> GetQueuedRomIds()
        {
            return _queueService.GetQueue()
                .Where(q => q.Roms.Any())
                .SelectMany(q => q.Roms.Select(e => e.Id))
                .ToList();
        }
    }
}
