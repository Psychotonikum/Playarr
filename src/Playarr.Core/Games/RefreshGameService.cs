using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Exceptions;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.MetadataSource;
using Playarr.Core.Games.Commands;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public class RefreshGameService : IExecute<RefreshSeriesCommand>
    {
        private readonly IProvideSeriesInfo _seriesInfo;
        private readonly IGameService _seriesService;
        private readonly IRefreshRomService _refreshRomService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfSeriesShouldBeRefreshed _checkIfSeriesShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly ICommandResultReporter _commandResultReporter;
        private readonly Logger _logger;

        public RefreshGameService(IProvideSeriesInfo seriesInfo,
                                    IGameService seriesService,
                                    IRefreshRomService refreshRomService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfSeriesShouldBeRefreshed checkIfSeriesShouldBeRefreshed,
                                    IConfigService configService,
                                    ICommandResultReporter commandResultReporter,
                                    Logger logger)
        {
            _seriesInfo = seriesInfo;
            _seriesService = seriesService;
            _refreshRomService = refreshRomService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfSeriesShouldBeRefreshed = checkIfSeriesShouldBeRefreshed;
            _configService = configService;
            _commandResultReporter = commandResultReporter;
            _logger = logger;
        }

        private Game RefreshSeriesInfo(int gameId)
        {
            // Get the game before updating, that way any changes made to the game after the refresh started,
            // but before this game was refreshed won't be lost.
            var game = _seriesService.GetSeries(gameId);

            _logger.ProgressInfo("Updating {0}", game.Title);

            Game seriesInfo;
            List<Rom> roms;

            try
            {
                var tuple = _seriesInfo.GetSeriesInfo(game.IgdbId);
                seriesInfo = tuple.Item1;
                roms = tuple.Item2;
            }
            catch (SeriesNotFoundException)
            {
                if (game.Status != GameStatusType.Deleted)
                {
                    game.Status = GameStatusType.Deleted;
                    _seriesService.UpdateSeries(game, publishUpdatedEvent: false);
                    _logger.Debug("Game marked as deleted on igdb for {0}", game.Title);
                    _eventAggregator.PublishEvent(new SeriesUpdatedEvent(game));
                }

                throw;
            }

            if (game.IgdbId != seriesInfo.IgdbId)
            {
                _logger.Warn("Game '{0}' (igdbid {1}) was replaced with '{2}' (igdbid {3}), because the original was a duplicate.", game.Title, game.IgdbId, seriesInfo.Title, seriesInfo.IgdbId);
                game.IgdbId = seriesInfo.IgdbId;
            }

            game.Title = seriesInfo.Title;
            game.Year = seriesInfo.Year;
            game.TitleSlug = seriesInfo.TitleSlug;
            game.MobyGamesId = seriesInfo.MobyGamesId;
            game.RawgId = seriesInfo.RawgId;
            game.TmdbId = seriesInfo.TmdbId;
            game.ImdbId = seriesInfo.ImdbId;
            game.MalIds = seriesInfo.MalIds;
            game.AniListIds = seriesInfo.AniListIds;
            game.AirTime = seriesInfo.AirTime;
            game.Overview = seriesInfo.Overview;
            game.OriginalLanguage = seriesInfo.OriginalLanguage;
            game.Status = seriesInfo.Status;
            game.CleanTitle = seriesInfo.CleanTitle;
            game.SortTitle = seriesInfo.SortTitle;
            game.LastInfoSync = DateTime.UtcNow;
            game.Runtime = seriesInfo.Runtime;
            game.Images = seriesInfo.Images;
            game.Network = seriesInfo.Network;
            game.FirstAired = seriesInfo.FirstAired;
            game.LastAired = seriesInfo.LastAired;
            game.Ratings = seriesInfo.Ratings;
            game.Actors = seriesInfo.Actors;
            game.Genres = seriesInfo.Genres;
            game.Certification = seriesInfo.Certification;
            game.OriginalCountry = seriesInfo.OriginalCountry;

            try
            {
                game.Path = new DirectoryInfo(game.Path).FullName;
                game.Path = game.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update game path for " + game.Path);
            }

            game.Platforms = UpdateSeasons(game, seriesInfo);

            _seriesService.UpdateSeries(game, publishUpdatedEvent: false);
            _refreshRomService.RefreshRomInfo(game, roms);

            _logger.Debug("Finished game refresh for {0}", game.Title);
            _eventAggregator.PublishEvent(new SeriesUpdatedEvent(game));

            return game;
        }

        private List<Platform> UpdateSeasons(Game game, Game seriesInfo)
        {
            var platforms = seriesInfo.Platforms.DistinctBy(s => s.PlatformNumber).ToList();

            foreach (var platform in platforms)
            {
                var existingSeason = game.Platforms.FirstOrDefault(s => s.PlatformNumber == platform.PlatformNumber);

                if (existingSeason == null)
                {
                    if (platform.PlatformNumber == 0)
                    {
                        _logger.Debug("Ignoring platform 0 for game [{0}] {1} by default", game.IgdbId, game.Title);
                        platform.Monitored = false;
                        continue;
                    }

                    var monitorNewSeasons = game.MonitorNewItems == NewItemMonitorTypes.All;

                    _logger.Debug("New platform ({0}) for game: [{1}] {2}, setting monitored to {3}", platform.PlatformNumber, game.IgdbId, game.Title, monitorNewSeasons.ToString().ToLowerInvariant());
                    platform.Monitored = monitorNewSeasons;
                }
                else
                {
                    platform.Monitored = existingSeason.Monitored;
                }
            }

            return platforms;
        }

        private void RescanSeries(Game game, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;

            if (isNew)
            {
                _logger.Trace("Forcing rescan of {0}. Reason: New game", game);
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: never rescan after refresh", game);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(game, SeriesScanSkippedReason.NeverRescanAfterRefresh));

                return;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: not after automatic scans", game);
                _eventAggregator.PublishEvent(new SeriesScanSkippedEvent(game, SeriesScanSkippedReason.RescanAfterManualRefreshOnly));

                return;
            }

            try
            {
                _diskScanService.Scan(game);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan game {0}", game);
            }
        }

        private void UpdateTags(Game game)
        {
            var tagsUpdated = _seriesService.UpdateTags(game);

            if (tagsUpdated)
            {
                _seriesService.UpdateSeries(game);
            }
        }

        public void Execute(RefreshSeriesCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewSeries;
            _eventAggregator.PublishEvent(new SeriesRefreshStartingEvent(trigger == CommandTrigger.Manual));

            if (message.GameIds.Any())
            {
                foreach (var gameId in message.GameIds)
                {
                    var game = _seriesService.GetSeries(gameId);

                    try
                    {
                        game = RefreshSeriesInfo(gameId);
                        UpdateTags(game);
                        RescanSeries(game, isNew, trigger);
                    }
                    catch (SeriesNotFoundException)
                    {
                        _logger.Error("Game '{0}' (igdbid {1}) was not found, it may have been removed from TheIGDB.", game.Title, game.IgdbId);

                        // Mark the result as indeterminate so it's not marked as a full success,
                        // // but we can still process other game if needed.
                        _commandResultReporter.Report(CommandResult.Indeterminate);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", game);
                        UpdateTags(game);
                        RescanSeries(game, isNew, trigger);

                        // Mark the result as indeterminate so it's not marked as a full success,
                        // but we can still process other game if needed.
                        _commandResultReporter.Report(CommandResult.Indeterminate);
                    }
                }
            }
            else
            {
                var allGames = _seriesService.GetAllSeries().OrderBy(c => c.SortTitle).ToList();

                foreach (var game in allGames)
                {
                    var seriesLocal = game;
                    if (trigger == CommandTrigger.Manual || _checkIfSeriesShouldBeRefreshed.ShouldRefresh(seriesLocal))
                    {
                        try
                        {
                            seriesLocal = RefreshSeriesInfo(seriesLocal.Id);
                        }
                        catch (SeriesNotFoundException)
                        {
                            _logger.Error("Game '{0}' (igdbid {1}) was not found, it may have been removed from TheIGDB.", seriesLocal.Title, seriesLocal.IgdbId);
                            continue;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", seriesLocal);
                        }

                        UpdateTags(game);
                        RescanSeries(seriesLocal, false, trigger);
                    }
                    else
                    {
                        _logger.Info("Skipping refresh of game: {0}", seriesLocal.Title);
                        UpdateTags(game);
                        RescanSeries(seriesLocal, false, trigger);
                    }
                }
            }

            _eventAggregator.PublishEvent(new SeriesRefreshCompleteEvent());
        }
    }
}
