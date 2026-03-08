using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public interface IRefreshRomService
    {
        void RefreshRomInfo(Game game, IEnumerable<Rom> remoteRoms);
    }

    public class RefreshRomService : IRefreshRomService
    {
        private readonly IRomService _episodeService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshRomService(IRomService episodeService, IEventAggregator eventAggregator, Logger logger)
        {
            _episodeService = episodeService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void RefreshRomInfo(Game game, IEnumerable<Rom> remoteRoms)
        {
            _logger.Info("Starting rom info refresh for: {0}", game);
            var successCount = 0;
            var failCount = 0;

            var existingRoms = _episodeService.GetEpisodeBySeries(game.Id);
            var platforms = game.Platforms;
            var hasExisting = existingRoms.Any();

            var updateList = new List<Rom>();
            var newList = new List<Rom>();
            var dupeFreeRemoteEpisodes = remoteRoms.DistinctBy(m => new { m.PlatformNumber, m.EpisodeNumber }).ToList();

            if (game.SeriesType == GameTypes.Anime)
            {
                dupeFreeRemoteEpisodes = MapAbsoluteRomNumbers(dupeFreeRemoteEpisodes);
            }

            var orderedEpisodes = OrderEpisodes(game, dupeFreeRemoteEpisodes).ToList();
            var episodesPerSeason = orderedEpisodes.GroupBy(s => s.PlatformNumber).ToDictionary(g => g.Key, g => g.Count());
            var latestSeason = platforms.MaxBy(s => s.PlatformNumber);

            foreach (var rom in orderedEpisodes)
            {
                try
                {
                    var episodeToUpdate = existingRoms.FirstOrDefault(e => e.PlatformNumber == rom.PlatformNumber && e.EpisodeNumber == rom.EpisodeNumber);

                    if (episodeToUpdate != null)
                    {
                        existingRoms.Remove(episodeToUpdate);
                        updateList.Add(episodeToUpdate);

                        // Anime game with newly added absolute rom number
                        if (game.SeriesType == GameTypes.Anime &&
                            !episodeToUpdate.AbsoluteEpisodeNumber.HasValue &&
                            rom.AbsoluteEpisodeNumber.HasValue)
                        {
                            episodeToUpdate.AbsoluteRomNumberAdded = true;
                        }
                    }
                    else
                    {
                        episodeToUpdate = new Rom();
                        episodeToUpdate.Monitored = GetMonitoredStatus(rom, platforms, game);
                        newList.Add(episodeToUpdate);
                    }

                    episodeToUpdate.GameId = game.Id;
                    episodeToUpdate.IgdbId = rom.IgdbId;
                    episodeToUpdate.EpisodeNumber = rom.EpisodeNumber;
                    episodeToUpdate.PlatformNumber = rom.PlatformNumber;
                    episodeToUpdate.AbsoluteEpisodeNumber = rom.AbsoluteEpisodeNumber;
                    episodeToUpdate.AiredAfterPlatformNumber = rom.AiredAfterPlatformNumber;
                    episodeToUpdate.AiredBeforePlatformNumber = rom.AiredBeforePlatformNumber;
                    episodeToUpdate.AiredBeforeRomNumber = rom.AiredBeforeRomNumber;
                    episodeToUpdate.Title = rom.Title ?? "TBA";
                    episodeToUpdate.Overview = rom.Overview;
                    episodeToUpdate.AirDate = rom.AirDate;
                    episodeToUpdate.AirDateUtc = rom.AirDateUtc;
                    episodeToUpdate.Runtime = rom.Runtime;
                    episodeToUpdate.FinaleType = rom.FinaleType;
                    episodeToUpdate.Ratings = rom.Ratings;
                    episodeToUpdate.Images = rom.Images;

                    // TheIGDB has a severe lack of platform/game finales, this helps smooth out that limitation so they can be displayed in the UI
                    if (game.Status == GameStatusType.Ended &&
                        episodeToUpdate.FinaleType == null &&
                        episodeToUpdate.PlatformNumber > 0 &&
                        episodeToUpdate.PlatformNumber == latestSeason.PlatformNumber &&
                        episodeToUpdate.EpisodeNumber > 1 &&
                        episodeToUpdate.EpisodeNumber == episodesPerSeason[episodeToUpdate.PlatformNumber] &&
                        episodeToUpdate.AirDateUtc.HasValue &&
                        episodeToUpdate.AirDateUtc.Value.After(DateTime.UtcNow.AddDays(-14)) &&
                        orderedEpisodes.None(e => e.PlatformNumber == latestSeason.PlatformNumber && e.FinaleType != null))
                    {
                        episodeToUpdate.FinaleType = "game";
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    _logger.Fatal(e, "An error has occurred while updating rom info for game {0}. {1}", game, rom);
                    failCount++;
                }
            }

            UnmonitorReaddedEpisodes(game, newList, hasExisting);

            var allEpisodes = new List<Rom>();
            allEpisodes.AddRange(newList);
            allEpisodes.AddRange(updateList);

            AdjustMultiEpisodeAirTime(game, allEpisodes);
            AdjustDirectToDvdAirDate(game, allEpisodes);

            _episodeService.DeleteMany(existingRoms);
            _episodeService.UpdateMany(updateList);
            _episodeService.InsertMany(newList);

            _eventAggregator.PublishEvent(new RomInfoRefreshedEvent(game, newList, updateList, existingRoms));

            if (failCount != 0)
            {
                _logger.Info("Finished rom refresh for game: {0}. Successful: {1} - Failed: {2} ",
                    game.Title,
                    successCount,
                    failCount);
            }
            else
            {
                _logger.Info("Finished rom refresh for game: {0}.", game);
            }
        }

        private bool GetMonitoredStatus(Rom rom, IEnumerable<Platform> platforms, Game game)
        {
            if (rom.EpisodeNumber == 0 && rom.PlatformNumber != 1)
            {
                return false;
            }

            var platform = platforms.SingleOrDefault(c => c.PlatformNumber == rom.PlatformNumber);
            return platform == null || platform.Monitored;
        }

        private void UnmonitorReaddedEpisodes(Game game, List<Rom> roms, bool hasExisting)
        {
            if (game.AddOptions != null)
            {
                return;
            }

            var threshold = DateTime.UtcNow.AddDays(-14);

            var oldEpisodes = roms.Where(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(threshold)).ToList();

            if (oldEpisodes.Any())
            {
                if (hasExisting)
                {
                    _logger.Warn("Show {0} ({1}) had {2} old roms appear, please check monitored status.", game.IgdbId, game.Title, oldEpisodes.Count);
                }
                else
                {
                    threshold = DateTime.UtcNow.AddDays(-1);

                    foreach (var rom in roms)
                    {
                        if (rom.AirDateUtc.HasValue && rom.AirDateUtc.Value.Before(threshold))
                        {
                            rom.Monitored = false;
                        }
                    }

                    _logger.Warn("Show {0} ({1}) had {2} old roms appear, unmonitored aired roms to prevent unexpected downloads.", game.IgdbId, game.Title, oldEpisodes.Count);
                }
            }
        }

        private void AdjustMultiEpisodeAirTime(Game game, IEnumerable<Rom> allEpisodes)
        {
            var groups = allEpisodes.Where(c => c.AirDateUtc.HasValue)
                                    .GroupBy(e => new { e.PlatformNumber, e.AirDate })
                                    .Where(g => g.Count() > 1)
                                    .ToList();

            foreach (var group in groups)
            {
                if (group.Key.PlatformNumber != 0 && group.Count() > 3)
                {
                    _logger.Debug("Not adjusting multi-rom air times for game {0} platform {1} since more than 3 roms 'aired' on the same day", game.Title, group.Key.PlatformNumber);
                    continue;
                }

                var episodeCount = 0;

                foreach (var rom in group.OrderBy(e => e.PlatformNumber).ThenBy(e => e.EpisodeNumber))
                {
                    rom.AirDateUtc = rom.AirDateUtc.Value.AddMinutes(game.Runtime * episodeCount);
                    episodeCount++;
                }
            }
        }

        private void AdjustDirectToDvdAirDate(Game game, IList<Rom> allEpisodes)
        {
            if (game.Status == GameStatusType.Ended && allEpisodes.All(v => !v.AirDateUtc.HasValue) && game.FirstAired.HasValue)
            {
                foreach (var rom in allEpisodes)
                {
                    rom.AirDateUtc = game.FirstAired;
                    rom.AirDate = game.FirstAired.Value.ToString("yyyy-MM-dd");
                }
            }
        }

        private List<Rom> MapAbsoluteRomNumbers(List<Rom> remoteRoms)
        {
            // Return all roms with no abs number, but distinct for those with abs number
            return remoteRoms.Where(e => e.AbsoluteEpisodeNumber.HasValue)
                                 .OrderByDescending(e => e.PlatformNumber)
                                 .DistinctBy(e => e.AbsoluteEpisodeNumber.Value)
                                 .Concat(remoteRoms.Where(e => !e.AbsoluteEpisodeNumber.HasValue))
                                 .ToList();
        }

        private IEnumerable<Rom> OrderEpisodes(Game game, List<Rom> roms)
        {
            if (game.SeriesType == GameTypes.Anime)
            {
                var withAbs = roms.Where(e => e.AbsoluteEpisodeNumber.HasValue)
                                      .OrderBy(e => e.AbsoluteEpisodeNumber);

                var withoutAbs = roms.Where(e => !e.AbsoluteEpisodeNumber.HasValue)
                                         .OrderBy(e => e.PlatformNumber)
                                         .ThenBy(e => e.EpisodeNumber);

                return withAbs.Concat(withoutAbs);
            }

            return roms.OrderBy(e => e.PlatformNumber).ThenBy(e => e.EpisodeNumber);
        }
    }
}
