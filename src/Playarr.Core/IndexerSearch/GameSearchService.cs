using System;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch
{
    public class GameSearchService : IExecute<GameSearchCommand>
    {
        private readonly IGameService _gameService;
        private readonly IRomService _romService;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public GameSearchService(IGameService seriesService,
                                   IRomService episodeService,
                                   ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _gameService = seriesService;
            _romService = episodeService;
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(GameSearchCommand message)
        {
            var game = _gameService.GetGame(message.GameId);
            var downloadedCount = 0;
            var userInvokedSearch = message.Trigger == CommandTrigger.Manual;
            var profile = game.QualityProfile.Value;

            if (game.Platforms.None(s => s.Monitored))
            {
                _logger.Debug("No platforms of {0} are monitored, searching for all monitored roms", game.Title);

                var roms = _romService.GetEpisodeBySeries(game.Id)
                    .Where(e => e.Monitored &&
                                !e.HasFile &&
                                e.AirDateUtc.HasValue &&
                                e.AirDateUtc.Value.Before(DateTime.UtcNow))
                    .ToList();

                foreach (var rom in roms)
                {
                    var decisions = _releaseSearchService.RomSearch(rom, userInvokedSearch, false).GetAwaiter().GetResult();
                    var processDecisions = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();
                    downloadedCount += processDecisions.Grabbed.Count;
                }
            }
            else
            {
                foreach (var platform in game.Platforms.OrderBy(s => s.PlatformNumber))
                {
                    if (!platform.Monitored)
                    {
                        _logger.Debug("Platform {0} of {1} is not monitored, skipping search", platform.PlatformNumber, game.Title);
                        continue;
                    }

                    var decisions = _releaseSearchService.PlatformSearch(message.GameId, platform.PlatformNumber, !profile.UpgradeAllowed, true, userInvokedSearch, false).GetAwaiter().GetResult();
                    var processDecisions = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();
                    downloadedCount += processDecisions.Grabbed.Count;
                }
            }

            _logger.ProgressInfo("Game search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
