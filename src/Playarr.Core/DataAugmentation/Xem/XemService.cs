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
        private readonly IRomService _episodeService;
        private readonly IXemProxy _xemProxy;
        private readonly IGameService _seriesService;
        private readonly Logger _logger;
        private readonly ICachedDictionary<bool> _cache;

        public XemService(IRomService episodeService,
                           IXemProxy xemProxy,
                           IGameService seriesService,
                           ICacheManager cacheManager,
                           Logger logger)
        {
            _episodeService = episodeService;
            _xemProxy = xemProxy;
            _seriesService = seriesService;
            _logger = logger;
            _cache = cacheManager.GetCacheDictionary<bool>(GetType(), "mappedTvdbid");
        }

        private void PerformUpdate(Game game)
        {
            _logger.Debug("Updating scene numbering mapping for: {0}", game);

            try
            {
                var mappings = _xemProxy.GetSceneTvdbMappings(game.TvdbId);

                if (!mappings.Any() && !game.UseSceneNumbering)
                {
                    _logger.Debug("Mappings for: {0} are empty, skipping", game);
                    return;
                }

                var roms = _episodeService.GetEpisodeBySeries(game.Id);

                foreach (var rom in roms)
                {
                    rom.SceneAbsoluteEpisodeNumber = null;
                    rom.SceneSeasonNumber = null;
                    rom.SceneEpisodeNumber = null;
                    rom.UnverifiedSceneNumbering = false;
                }

                foreach (var mapping in mappings)
                {
                    _logger.Debug("Setting scene numbering mappings for {0} S{1:00}E{2:00}", game, mapping.Tvdb.Platform, mapping.Tvdb.Rom);

                    var rom = roms.SingleOrDefault(e => e.SeasonNumber == mapping.Tvdb.Platform && e.EpisodeNumber == mapping.Tvdb.Rom);

                    if (rom == null)
                    {
                        _logger.Debug("Information hasn't been added to TheIGDB yet, skipping");
                        continue;
                    }

                    if (mapping.Scene.Absolute == 0 &&
                        mapping.Scene.Platform == 0 &&
                        mapping.Scene.Rom == 0)
                    {
                        _logger.Debug("Mapping for {0} S{1:00}E{2:00} is invalid, skipping", game, mapping.Tvdb.Platform, mapping.Tvdb.Rom);
                        continue;
                    }

                    rom.SceneAbsoluteEpisodeNumber = mapping.Scene.Absolute;
                    rom.SceneSeasonNumber = mapping.Scene.Platform;
                    rom.SceneEpisodeNumber = mapping.Scene.Rom;
                }

                if (roms.Any(v => v.SceneEpisodeNumber.HasValue && v.SceneSeasonNumber != 0))
                {
                    ExtrapolateMappings(game, roms, mappings);
                }

                _episodeService.UpdateEpisodes(roms);
                game.UseSceneNumbering = mappings.Any();
                _seriesService.UpdateSeries(game);

                _logger.Debug("XEM mapping updated for {0}", game);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating scene numbering mappings for {0}", game);
            }
        }

        private void ExtrapolateMappings(Game game, List<Rom> roms, List<Model.XemSceneTvdbMapping> mappings)
        {
            var mappedEpisodes = roms.Where(v => v.SeasonNumber != 0 && v.SceneEpisodeNumber.HasValue).ToList();
            var mappedSeasons = new HashSet<int>(mappedEpisodes.Select(v => v.SeasonNumber).Distinct());

            var sceneEpisodeMappings = mappings.ToLookup(v => v.Scene.Platform)
                                               .ToDictionary(v => v.Key, e => new HashSet<int>(e.Select(v => v.Scene.Rom)));

            var firstTvdbEpisodeBySeason = mappings.ToLookup(v => v.Tvdb.Platform)
                                                   .ToDictionary(v => v.Key, e => e.Min(v => v.Tvdb.Rom));

            var lastSceneSeason = mappings.Select(v => v.Scene.Platform).Max();
            var lastTvdbSeason = mappings.Select(v => v.Tvdb.Platform).Max();

            // Mark all roms not on the xem as unverified.
            foreach (var rom in roms)
            {
                if (rom.SeasonNumber == 0)
                {
                    continue;
                }

                if (rom.SceneEpisodeNumber.HasValue)
                {
                    continue;
                }

                if (mappedSeasons.Contains(rom.SeasonNumber))
                {
                    // Mark if a mapping exists for an earlier rom in this platform.
                    if (firstTvdbEpisodeBySeason[rom.SeasonNumber] <= rom.EpisodeNumber)
                    {
                        rom.UnverifiedSceneNumbering = true;
                        continue;
                    }

                    // Mark if a mapping exists with a scene number to this rom.
                    if (sceneEpisodeMappings.ContainsKey(rom.SeasonNumber) &&
                        sceneEpisodeMappings[rom.SeasonNumber].Contains(rom.EpisodeNumber))
                    {
                        rom.UnverifiedSceneNumbering = true;
                        continue;
                    }
                }
                else if (lastSceneSeason != lastTvdbSeason && rom.SeasonNumber > lastTvdbSeason)
                {
                    rom.UnverifiedSceneNumbering = true;
                }
            }

            foreach (var rom in roms)
            {
                if (rom.SeasonNumber == 0)
                {
                    continue;
                }

                if (rom.SceneEpisodeNumber.HasValue)
                {
                    continue;
                }

                if (rom.SeasonNumber < lastTvdbSeason)
                {
                    continue;
                }

                if (!rom.UnverifiedSceneNumbering)
                {
                    continue;
                }

                var seasonMappings = mappings.Where(v => v.Tvdb.Platform == rom.SeasonNumber).ToList();
                if (seasonMappings.Any(v => v.Tvdb.Rom >= rom.EpisodeNumber))
                {
                    continue;
                }

                if (seasonMappings.Any())
                {
                    var lastEpisodeMapping = seasonMappings.OrderBy(v => v.Tvdb.Rom).Last();
                    var lastSceneSeasonMapping = mappings.Where(v => v.Scene.Platform == lastEpisodeMapping.Scene.Platform).OrderBy(v => v.Scene.Rom).Last();

                    if (lastSceneSeasonMapping.Tvdb.Platform == 0)
                    {
                        continue;
                    }

                    var offset = rom.EpisodeNumber - lastEpisodeMapping.Tvdb.Rom;

                    rom.SceneSeasonNumber = lastEpisodeMapping.Scene.Platform;
                    rom.SceneEpisodeNumber = lastEpisodeMapping.Scene.Rom + offset;
                    rom.SceneAbsoluteEpisodeNumber = lastEpisodeMapping.Scene.Absolute + offset;
                }
                else if (lastTvdbSeason != lastSceneSeason)
                {
                    var offset = rom.SeasonNumber - lastTvdbSeason;

                    rom.SceneSeasonNumber = lastSceneSeason + offset;
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
            var mappings = _xemProxy.GetSceneTvdbNames();

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

            if (!_cache.Find(message.Game.TvdbId.ToString()) && !message.Game.UseSceneNumbering)
            {
                _logger.Debug("Scene numbering is not available for {0} [{1}]", message.Game.Title, message.Game.TvdbId);
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
