using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Cache;
using Playarr.Core.Configuration;
using Playarr.Core.Datastore;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public interface IRomService
    {
        Rom GetEpisode(int id);
        List<Rom> GetRoms(IEnumerable<int> ids);
        Rom FindEpisode(int gameId, int platformNumber, int romNumber);
        Rom FindEpisode(int gameId, int absoluteRomNumber);
        Rom FindEpisodeByTitle(int gameId, int platformNumber, string releaseTitle);
        List<Rom> FindEpisodesBySceneNumbering(int gameId, int platformNumber, int romNumber);
        List<Rom> FindEpisodesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber);
        Rom FindEpisode(int gameId, string date, int? part);
        List<Rom> GetEpisodeBySeries(int gameId);
        List<Rom> GetRomsByGame(List<int> gameIds);
        List<Rom> GetRomsByPlatform(int gameId, int platformNumber);
        List<Rom> GetRomsByScenePlatform(int gameId, int scenePlatformNumber);
        List<Rom> EpisodesWithFiles(int gameId);
        PagingSpec<Rom> EpisodesWithoutFiles(PagingSpec<Rom> pagingSpec);
        List<Rom> GetRomsByFileId(int romFileId);
        void UpdateEpisode(Rom rom);
        void SetEpisodeMonitored(int romId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        void UpdateEpisodes(List<Rom> roms);
        void UpdateLastSearchTime(List<Rom> roms);
        List<Rom> EpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored, bool includeSpecials);
        void InsertMany(List<Rom> roms);
        void UpdateMany(List<Rom> roms);
        void DeleteMany(List<Rom> roms);
        void SetEpisodeMonitoredBySeason(int gameId, int platformNumber, bool monitored);
    }

    public class RomService : IRomService,
                                  IHandle<RomFileDeletedEvent>,
                                  IHandle<RomFileAddedEvent>,
                                  IHandleAsync<SeriesDeletedEvent>,
                                  IHandleAsync<SeriesScannedEvent>
    {
        private readonly IRomRepository _romRepository;
        private readonly IConfigService _configService;
        private readonly ICached<HashSet<int>> _cache;
        private readonly Logger _logger;

        public RomService(IRomRepository episodeRepository, IConfigService configService, ICacheManager cacheManager, Logger logger)
        {
            _romRepository = episodeRepository;
            _configService = configService;
            _cache = cacheManager.GetCache<HashSet<int>>(GetType());
            _logger = logger;
        }

        public Rom GetEpisode(int id)
        {
            return _romRepository.Get(id);
        }

        public List<Rom> GetRoms(IEnumerable<int> ids)
        {
            return _romRepository.Get(ids).ToList();
        }

        public Rom FindEpisode(int gameId, int platformNumber, int romNumber)
        {
            return _romRepository.Find(gameId, platformNumber, romNumber);
        }

        public Rom FindEpisode(int gameId, int absoluteRomNumber)
        {
            return _romRepository.Find(gameId, absoluteRomNumber);
        }

        public List<Rom> FindEpisodesBySceneNumbering(int gameId, int platformNumber, int romNumber)
        {
            return _romRepository.FindEpisodesBySceneNumbering(gameId, platformNumber, romNumber);
        }

        public List<Rom> FindEpisodesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber)
        {
            return _romRepository.FindEpisodesBySceneNumbering(gameId, sceneAbsoluteRomNumber);
        }

        public Rom FindEpisode(int gameId, string date, int? part)
        {
            return FindOneByAirDate(gameId, date, part);
        }

        public List<Rom> GetEpisodeBySeries(int gameId)
        {
            return _romRepository.GetRoms(gameId).ToList();
        }

        public List<Rom> GetRomsByGame(List<int> gameIds)
        {
            return _romRepository.GetRomsByGameIds(gameIds).ToList();
        }

        public List<Rom> GetRomsByPlatform(int gameId, int platformNumber)
        {
            return _romRepository.GetRoms(gameId, platformNumber);
        }

        public List<Rom> GetRomsByScenePlatform(int gameId, int scenePlatformNumber)
        {
            return _romRepository.GetRomsByScenePlatform(gameId, scenePlatformNumber);
        }

        public Rom FindEpisodeByTitle(int gameId, int platformNumber, string releaseTitle)
        {
            // TODO: can replace this search mechanism with something smarter/faster/better
            var normalizedReleaseTitle = Parser.Parser.NormalizeRomTitle(releaseTitle);
            var cleanNormalizedReleaseTitle = Parser.Parser.CleanGameTitle(normalizedReleaseTitle);
            var roms = _romRepository.GetRoms(gameId, platformNumber);

            var possibleMatches = roms.SelectMany(
                rom => new[]
                {
                    new
                    {
                        Position = normalizedReleaseTitle.IndexOf(Parser.Parser.NormalizeRomTitle(rom.Title), StringComparison.CurrentCultureIgnoreCase),
                        Length = Parser.Parser.NormalizeRomTitle(rom.Title).Length,
                        Rom = rom
                    },
                    new
                    {
                        Position = cleanNormalizedReleaseTitle.IndexOf(Parser.Parser.CleanGameTitle(Parser.Parser.NormalizeRomTitle(rom.Title)), StringComparison.CurrentCultureIgnoreCase),
                        Length = Parser.Parser.NormalizeRomTitle(rom.Title).Length,
                        Rom = rom
                    }
                });

            var matches = possibleMatches
                                .Where(e => e.Rom.Title.Length > 0 && e.Position >= 0)
                                .OrderBy(e => e.Position)
                                .ThenByDescending(e => e.Length)
                                .ToList();

            if (matches.Any())
            {
                return matches.First().Rom;
            }

            return null;
        }

        public List<Rom> EpisodesWithFiles(int gameId)
        {
            return _romRepository.EpisodesWithFiles(gameId);
        }

        public PagingSpec<Rom> EpisodesWithoutFiles(PagingSpec<Rom> pagingSpec)
        {
            var episodeResult = _romRepository.EpisodesWithoutFiles(pagingSpec, true);

            return episodeResult;
        }

        public List<Rom> GetRomsByFileId(int romFileId)
        {
            return _romRepository.GetEpisodeByFileId(romFileId);
        }

        public void UpdateEpisode(Rom rom)
        {
            _romRepository.Update(rom);
        }

        public void SetEpisodeMonitored(int romId, bool monitored)
        {
            var rom = _romRepository.Get(romId);
            _romRepository.SetMonitoredFlat(rom, monitored);

            _logger.Debug("Monitored flag for Rom:{0} was set to {1}", romId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _romRepository.SetMonitored(ids, monitored);
        }

        public void SetEpisodeMonitoredBySeason(int gameId, int platformNumber, bool monitored)
        {
            _romRepository.SetMonitoredBySeason(gameId, platformNumber, monitored);
        }

        public void UpdateEpisodes(List<Rom> roms)
        {
            _romRepository.UpdateMany(roms);
        }

        public void UpdateLastSearchTime(List<Rom> roms)
        {
            _romRepository.SetFields(roms, e => e.LastSearchTime);
        }

        public List<Rom> EpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored, bool includeSpecials)
        {
            var roms = _romRepository.EpisodesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored, includeSpecials);

            return roms;
        }

        public void InsertMany(List<Rom> roms)
        {
            _romRepository.InsertMany(roms);
        }

        public void UpdateMany(List<Rom> roms)
        {
            _romRepository.UpdateMany(roms);
        }

        public void DeleteMany(List<Rom> roms)
        {
            _romRepository.DeleteMany(roms);
        }

        private Rom FindOneByAirDate(int gameId, string date, int? part)
        {
            var roms = _romRepository.Find(gameId, date);

            if (!roms.Any())
            {
                return null;
            }

            if (roms.Count == 1)
            {
                return roms.First();
            }

            _logger.Debug("Multiple roms with the same air date were found, will exclude specials");

            var regularEpisodes = roms.Where(e => e.PlatformNumber > 0).ToList();

            if (regularEpisodes.Count == 1 && !part.HasValue)
            {
                _logger.Debug("Left with one rom after excluding specials");
                return regularEpisodes.First();
            }
            else if (part.HasValue && part.Value <= regularEpisodes.Count)
            {
                var sortedEpisodes = regularEpisodes.OrderBy(e => e.PlatformNumber)
                                                               .ThenBy(e => e.EpisodeNumber)
                                                                .ToList();

                return sortedEpisodes[part.Value - 1];
            }

            throw new InvalidOperationException($"Multiple roms with the same air date found. Date: {date}");
        }

        public void Handle(RomFileDeletedEvent message)
        {
            foreach (var rom in GetRomsByFileId(message.RomFile.Id))
            {
                _logger.Debug("Detaching rom {0} from file.", rom.Id);

                var unmonitorEpisodes = _configService.AutoUnmonitorPreviouslyDownloadedEpisodes;

                var unmonitorForReason = message.Reason != DeleteMediaFileReason.Upgrade &&
                                         message.Reason != DeleteMediaFileReason.ManualOverride &&
                                         message.Reason != DeleteMediaFileReason.MissingFromDisk;

                // If rom is being unlinked because it's missing from disk store it for
                if (message.Reason == DeleteMediaFileReason.MissingFromDisk && unmonitorEpisodes)
                {
                    lock (_cache)
                    {
                        var ids = _cache.Get(rom.GameId.ToString(), () => new HashSet<int>());

                        ids.Add(rom.Id);
                    }
                }

                _romRepository.ClearFileId(rom, unmonitorForReason && unmonitorEpisodes);
            }
        }

        public void Handle(RomFileAddedEvent message)
        {
            foreach (var rom in message.RomFile.Roms.Value)
            {
                _romRepository.SetFileId(rom, message.RomFile.Id);

                lock (_cache)
                {
                    var ids = _cache.Find(rom.GameId.ToString());

                    if (ids?.Contains(rom.Id) == true)
                    {
                        ids.Remove(rom.Id);
                    }
                }

                _logger.Debug("Linking [{0}] > [{1}]", message.RomFile.RelativePath, rom);
            }
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var roms = _romRepository.GetRomsByGameIds(message.Game.Select(s => s.Id).ToList());
            _romRepository.DeleteMany(roms);
        }

        public void HandleAsync(SeriesScannedEvent message)
        {
            lock (_cache)
            {
                var ids = _cache.Find(message.Game.Id.ToString());

                if (ids?.Any() == true)
                {
                    _romRepository.SetMonitored(ids, false);
                }

                _cache.Remove(message.Game.Id.ToString());
            }
        }
    }
}
