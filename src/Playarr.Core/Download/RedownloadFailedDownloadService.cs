using System.Linq;
using NLog;
using Playarr.Core.Configuration;
using Playarr.Core.IndexerSearch;
using Playarr.Core.Messaging;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Download
{
    public class RedownloadFailedDownloadService : IHandle<DownloadFailedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IRomService _romService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public RedownloadFailedDownloadService(IConfigService configService,
                                               IRomService episodeService,
                                               IManageCommandQueue commandQueueManager,
                                               Logger logger)
        {
            _configService = configService;
            _romService = episodeService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(DownloadFailedEvent message)
        {
            if (message.SkipRedownload)
            {
                _logger.Debug("Skip redownloading requested by user");
                return;
            }

            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Debug("Auto redownloading failed roms is disabled");
                return;
            }

            if (message.ReleaseSource == ReleaseSourceType.InteractiveSearch && !_configService.AutoRedownloadFailedFromInteractiveSearch)
            {
                _logger.Debug("Auto redownloading failed roms from interactive search is disabled");
                return;
            }

            if (message.RomIds.Count == 1)
            {
                _logger.Debug("Failed download only contains one rom, searching again");

                _commandQueueManager.Push(new RomSearchCommand(message.RomIds));

                return;
            }

            var platformNumber = _romService.GetEpisode(message.RomIds.First()).PlatformNumber;
            var episodesInSeason = _romService.GetRomsByPlatform(message.GameId, platformNumber);

            if (message.RomIds.Count == episodesInSeason.Count)
            {
                _logger.Debug("Failed download was entire platform, searching again");

                _commandQueueManager.Push(new PlatformSearchCommand
                {
                    GameId = message.GameId,
                    PlatformNumber = platformNumber
                });

                return;
            }

            _logger.Debug("Failed download contains multiple roms, probably a double rom, searching again");

            _commandQueueManager.Push(new RomSearchCommand(message.RomIds));
        }
    }
}
