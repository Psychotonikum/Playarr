using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;

namespace Playarr.Core.Games
{
    public interface IEpisodeMonitoredService
    {
        void SetEpisodeMonitoredStatus(Game game, MonitoringOptions monitoringOptions);
    }

    public class EpisodeMonitoredService : IEpisodeMonitoredService
    {
        private readonly IGameService _seriesService;
        private readonly IRomService _episodeService;
        private readonly Logger _logger;

        public EpisodeMonitoredService(IGameService seriesService, IRomService episodeService, Logger logger)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void SetEpisodeMonitoredStatus(Game game, MonitoringOptions monitoringOptions)
        {
            // Update the game without changing the roms
            if (monitoringOptions == null)
            {
                _seriesService.UpdateSeries(game, false);
                return;
            }

            // Fallback for v2 endpoints
            if (monitoringOptions.Monitor == MonitorTypes.Unknown)
            {
                LegacySetEpisodeMonitoredStatus(game, monitoringOptions);
                return;
            }

            // Skip rom level monitoring and use platform information when game was added
            if (monitoringOptions.Monitor == MonitorTypes.Skip)
            {
                return;
            }

            var firstSeason = game.Platforms.Select(s => s.SeasonNumber).Where(s => s > 0).MinOrDefault();
            var lastSeason = game.Platforms.Select(s => s.SeasonNumber).MaxOrDefault();
            var roms = _episodeService.GetEpisodeBySeries(game.Id);

            switch (monitoringOptions.Monitor)
            {
                case MonitorTypes.All:
                    _logger.Debug("[{0}] Monitoring all roms", game.Title);
                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0);

                    break;

                case MonitorTypes.Future:
                    _logger.Debug("[{0}] Monitoring future roms", game.Title);
                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0 && (!e.AirDateUtc.HasValue || e.AirDateUtc >= DateTime.UtcNow));

                    break;

                case MonitorTypes.Missing:
                    _logger.Debug("[{0}] Monitoring missing roms", game.Title);
                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0 && !e.HasFile);

                    break;

                case MonitorTypes.Existing:
                    _logger.Debug("[{0}] Monitoring existing roms", game.Title);
                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0 && e.HasFile);

                    break;

                case MonitorTypes.Pilot:
                    _logger.Debug("[{0}] Monitoring first rom roms", game.Title);
                    ToggleEpisodesMonitoredState(roms,
                        e => e.SeasonNumber > 0 && e.SeasonNumber == firstSeason && e.EpisodeNumber == 1);

                    break;

                case MonitorTypes.FirstSeason:
                    _logger.Debug("[{0}] Monitoring first platform roms", game.Title);
                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0 && e.SeasonNumber == firstSeason);

                    break;

                case MonitorTypes.LastSeason:
                #pragma warning disable CS0612
                case MonitorTypes.LatestSeason:
                #pragma warning restore CS0612
                    _logger.Debug("[{0}] Monitoring latest platform roms", game.Title);

                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0 && e.SeasonNumber == lastSeason);

                    break;

                case MonitorTypes.Recent:
                    _logger.Debug("[{0}] Monitoring recent and future roms", game.Title);

                    ToggleEpisodesMonitoredState(roms, e => e.SeasonNumber > 0 &&
                                                                (!e.AirDateUtc.HasValue || (
                                                                        e.AirDateUtc.Value.Before(DateTime.UtcNow) &&
                                                                        e.AirDateUtc.Value.InLastDays(90))
                                                                    || e.AirDateUtc.Value.After(DateTime.UtcNow)));

                    break;

                case MonitorTypes.MonitorSpecials:
                    _logger.Debug("[{0}] Monitoring special roms", game.Title);
                    ToggleEpisodesMonitoredState(roms.Where(e => e.SeasonNumber == 0), true);

                    break;

                case MonitorTypes.UnmonitorSpecials:
                    _logger.Debug("[{0}] Unmonitoring special roms", game.Title);
                    ToggleEpisodesMonitoredState(roms.Where(e => e.SeasonNumber == 0), false);

                    break;

                case MonitorTypes.None:
                    _logger.Debug("[{0}] Unmonitoring all roms", game.Title);
                    ToggleEpisodesMonitoredState(roms, e => false);

                    break;
            }

            var monitoredSeasons = roms.Where(e => e.Monitored)
                                           .Select(e => e.SeasonNumber)
                                           .Distinct()
                                           .ToList();

            foreach (var platform in game.Platforms)
            {
                var platformNumber = platform.SeasonNumber;

                // Monitor the last platform when:
                // - Not specials
                // - The latest platform
                // - Set to monitor all roms
                // - Set to monitor future roms and game is continuing or not yet aired
                if (platformNumber > 0 &&
                    platformNumber == lastSeason &&
                    (monitoringOptions.Monitor == MonitorTypes.All ||
                     (monitoringOptions.Monitor == MonitorTypes.Future && game.Status is GameStatusType.Continuing or GameStatusType.Upcoming)))
                {
                    platform.Monitored = true;
                }
                else if (platformNumber == firstSeason && monitoringOptions.Monitor == MonitorTypes.Pilot)
                {
                    // Don't monitor platform 1 if only the pilot rom is monitored
                    platform.Monitored = false;
                }
                else if (monitoredSeasons.Contains(platformNumber))
                {
                    // Monitor the platform if it has any monitor roms
                    platform.Monitored = true;
                }

                // Don't monitor the platform
                else
                {
                    platform.Monitored = false;
                }
            }

            _episodeService.UpdateEpisodes(roms);
            _seriesService.UpdateSeries(game, false);
        }

        private void LegacySetEpisodeMonitoredStatus(Game game, MonitoringOptions monitoringOptions)
        {
            _logger.Debug("[{0}] Setting rom monitored status.", game.Title);

            var roms = _episodeService.GetEpisodeBySeries(game.Id);

            if (monitoringOptions.IgnoreEpisodesWithFiles)
            {
                _logger.Debug("Unmonitoring Roms with Files");
                ToggleEpisodesMonitoredState(roms.Where(e => e.HasFile), false);
            }
            else
            {
                _logger.Debug("Monitoring Roms with Files");
                ToggleEpisodesMonitoredState(roms.Where(e => e.HasFile), true);
            }

            if (monitoringOptions.IgnoreEpisodesWithoutFiles)
            {
                _logger.Debug("Unmonitoring Roms without Files");
                ToggleEpisodesMonitoredState(roms.Where(e => !e.HasFile && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)), false);
            }
            else
            {
                _logger.Debug("Monitoring Roms without Files");
                ToggleEpisodesMonitoredState(roms.Where(e => !e.HasFile && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)), true);
            }

            var lastSeason = game.Platforms.Select(s => s.SeasonNumber).MaxOrDefault();

            foreach (var s in game.Platforms)
            {
                var platform = s;

                // If the platform is unmonitored we should unmonitor all roms in that platform

                if (!platform.Monitored)
                {
                    _logger.Debug("Unmonitoring all roms in platform {0}", platform.SeasonNumber);
                    ToggleEpisodesMonitoredState(roms.Where(e => e.SeasonNumber == platform.SeasonNumber), false);
                }

                // If the platform is not the latest platform and all it's roms are unmonitored the platform will be unmonitored

                if (platform.SeasonNumber < lastSeason)
                {
                    if (roms.Where(e => e.SeasonNumber == platform.SeasonNumber).All(e => !e.Monitored))
                    {
                        _logger.Debug("Unmonitoring platform {0} because all roms are not monitored", platform.SeasonNumber);
                        platform.Monitored = false;
                    }
                }
            }

            _episodeService.UpdateEpisodes(roms);

            _seriesService.UpdateSeries(game, false);
        }

        private void ToggleEpisodesMonitoredState(IEnumerable<Rom> roms, bool monitored)
        {
            foreach (var rom in roms)
            {
                rom.Monitored = monitored;
            }
        }

        private void ToggleEpisodesMonitoredState(List<Rom> roms, Func<Rom, bool> predicate)
        {
            ToggleEpisodesMonitoredState(roms.Where(predicate), true);
            ToggleEpisodesMonitoredState(roms.Where(e => !predicate(e)), false);
        }
    }
}
