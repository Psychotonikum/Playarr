using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Cache;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.DataAugmentation.Xem
{
    public class XemService : ISceneMappingProvider, IHandle<SeriesUpdatedEvent>, IHandle<SeriesRefreshStartingEvent>
    {
        private readonly IRomService _romService;
        private readonly IXemProxy _xemProxy;
        private readonly IGameService _gameService;
        private readonly Logger _logger;
        private readonly ICachedDictionary<bool> _cache;

        public XemService(IRomService episodeService,
                           IXemProxy xemProxy,
                           IGameService seriesService,
                           ICacheManager cacheManager,
                           Logger logger)
        {
            _romService = episodeService;
            _xemProxy = xemProxy;
            _gameService = seriesService;
            _logger = logger;
            _cache = cacheManager.GetCacheDictionary<bool>(GetType(), "mappedIgdbid");
        }

        private void PerformUpdate(Game game)
        {
            _logger.Debug("Updating scene numbering mapping for: {0}", game);

            try
            {
                var mappings = _xemProxy.GetSceneIgdbMappings(game.IgdbId);

                if (!mappings.Any() && !game.UseSceneNumbering)
                {
                    _logger.Debug("Mappings for: {0} are empty, skipping", game);
                    return;
                }

                var roms = _romService.GetEpisodeBySeries(game.Id);

                foreach (var rom in roms)
                {
                    rom.SceneAbsoluteEpisodeNumber = null;
                    rom.ScenePlatformNumber = null;
                    rom.SceneEpisodeNumber = null;
                    rom.UnverifiedSceneNumbering = false;
                }

                foreach (var mapping in mappings)
                {
                    _logger.Debug("Setting scene numbering mappings for {0} S{1:00}E{2:00}", game, mapping.Igdb.Platform, mapping.Igdb.Rom);

                    var rom = roms.SingleOrDefault(e => e.PlatformNumber == mapping.Igdb.Platform && e.EpisodeNumber == mapping.Igdb.Rom);

                    if (rom == null)
                    {
                        _logger.Debug("Information hasn't been added to TheIGDB yet, skipping");
                        continue;
                    }

                    if (mapping.Scene.Absolute == 0 &&
                        mapping.Scene.Platform == 0 &&
                        mapping.Scene.Rom == 0)
                    {
                        _logger.Debug("Mapping for {0} S{1:00}E{2:00} is invalid, skipping", game, mapping.Igdb.Platform, mapping.Igdb.Rom);
                        continue;
                    }

                    rom.SceneAbsoluteEpisodeNumber = mapping.Scene.Absolute;
                    rom.ScenePlatformNumber = mapping.Scene.Platform;
                    rom.SceneEpisodeNumber = mapping.Scene.Rom;
                }

                if (roms.Any(v => v.SceneEpisodeNumber.HasValue && v.ScenePlatformNumber != 0))
                {
                    ExtrapolateMappings(game, roms, mappings);
                }

                _romService.UpdateEpisodes(roms);
                game.UseSceneNumbering = mappings.Any();
                _gameService.UpdateSeries(game);

                _logger.Debug("XEM mapping updated for {0}", game);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating scene numbering mappings for {0}", game);
            }
        }

        private void ExtrapolateMappings(Game game, List<Rom> roms, List<Model.XemSceneIgdbMapping> mappings)
        {
            var mappedEpisodes = roms.Where(v => v.PlatformNumber != 0 && v.SceneEpisodeNumber.HasValue).ToList();
            var mappedSeasons = new HashSet<int>(mappedEpisodes.Select(v => v.PlatformNumber).Distinct());

            var sceneEpisodeMappings = mappings.ToLookup(v => v.Scene.Platform)
                                               .ToDictionary(v => v.Key, e => new HashSet<int>(e.Select(v => v.Scene.Rom)));

            var firstIgdbEpisodeBySeason = mappings.ToLookup(v => v.Igdb.Platform)
                                                   .ToDictionary(v => v.Key, e => e.Min(v => v.Igdb.Rom));

            var lastSceneSeason = mappings.Select(v => v.Scene.Platform).Max();
            var lastIgdbSeason = mappings.Select(v => v.Igdb.Platform).Max();

            // Mark all roms not on the xem as unverified.
            foreach (var rom in roms)
            {
                if (rom.PlatformNumber == 0)
                {
                    continue;
                }

                if (rom.SceneEpisodeNumber.HasValue)
                {
                    continue;
                }

                if (mappedSeasons.Contains(rom.PlatformNumber))
                {
                    // Mark if a mapping exists for an earlier rom in this platform.
                    if (firstIgdbEpisodeBySeason[rom.PlatformNumber] <= rom.EpisodeNumber)
                    {
                        rom.UnverifiedSceneNumbering = true;
                        continue;
                    }

                    // Mark if a mapping exists with a scene number to this rom.
                    if (sceneEpisodeMappings.ContainsKey(rom.PlatformNumber) &&
                        sceneEpisodeMappings[rom.PlatformNumber].Contains(rom.EpisodeNumber))
                    {
                        rom.UnverifiedSceneNumbering = true;
                        continue;
                    }
                }
                else if (lastSceneSeason != lastIgdbSeason && rom.PlatformNumber > lastIgdbSeason)
                {
                    rom.UnverifiedSceneNumbering = true;
                }
            }

            foreach (var rom in roms)
            {
                if (rom.PlatformNumber == 0)
                {
                    continue;
                }

                if (rom.SceneEpisodeNumber.HasValue)
                {
                    continue;
                }

                if (rom.PlatformNumber < lastIgdbSeason)
                {
                    continue;
                }

                if (!rom.UnverifiedSceneNumbering)
                {
                    continue;
                }

                var seasonMappings = mappings.Where(v => v.Igdb.Platform == rom.PlatformNumber).ToList();
                if (seasonMappings.Any(v => v.Igdb.Rom >= rom.EpisodeNumber))
                {
                    continue;
                }

                if (seasonMappings.Any())
                {
                    var lastEpisodeMapping = seasonMappings.OrderBy(v => v.Igdb.Rom).Last();
                    var lastSceneSeasonMapping = mappings.Where(v => v.Scene.Platform == lastEpisodeMapping.Scene.Platform).OrderBy(v => v.Scene.Rom).Last();

                    if (lastSceneSeasonMapping.Igdb.Platform == 0)
                    {
                        continue;
                    }

                    var offset = rom.EpisodeNumber - lastEpisodeMapping.Igdb.Rom;

                    rom.ScenePlatformNumber = lastEpisodeMapping.Scene.Platform;
                    rom.SceneEpisodeNumber = lastEpisodeMapping.Scene.Rom + offset;
                    rom.SceneAbsoluteEpisodeNumber = lastEpisodeMapping.Scene.Absolute + offset;
                }
                else if (lastIgdbSeason != lastSceneSeason)
                {
                    var offset = rom.PlatformNumber - lastIgdbSeason;

                    rom.ScenePlatformNumber = lastSceneSeason + offset;
                    rom.SceneEpisodeNumber = rom.EpisodeNumber;

                    // TODO: SceneAbsoluteEpisodeNumber.
                }
            }
        }

        private void UpdateXemGameIds()
        {
            try
            {
                var ids = _xemProxy.GetXemGameIds();

                if (ids.Any())
                {
                    _cache.Update(ids.ToDictionary(v => v.ToString(), v => true));
                    return;
                }

                _cache.ExtendTTL();
                _logger.Warn("Failed to update Xem game list.");
            }
            catch (Exception ex)
            {
                _cache.ExtendTTL();
                _logger.Warn(ex, "Failed to update Xem game list.");
            }
        }

        public List<SceneMapping> GetSceneMappings()
        {
            var mappings = _xemProxy.GetSceneIgdbNames();

            return mappings;
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            if (_cache.IsExpired(TimeSpan.FromHours(3)))
            {
                UpdateXemGameIds();
            }

            if (_cache.Count == 0)
            {
                _logger.Debug("Scene numbering is not available");
                return;
            }

            if (!_cache.Find(message.Game.IgdbId.ToString()) && !message.Game.UseSceneNumbering)
            {
                _logger.Debug("Scene numbering is not available for {0} [{1}]", message.Game.Title, message.Game.IgdbId);
                return;
            }

            PerformUpdate(message.Game);
        }

        public void Handle(SeriesRefreshStartingEvent message)
        {
            if (message.ManualTrigger && _cache.IsExpired(TimeSpan.FromMinutes(1)))
            {
                UpdateXemGameIds();
            }
        }
    }
}
