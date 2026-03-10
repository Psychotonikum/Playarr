using NLog;
using Playarr.Core.IndexerSearch;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public class SeriesScannedHandler : IHandle<SeriesScannedEvent>,
                                        IHandle<SeriesScanSkippedEvent>
    {
        private readonly IEpisodeMonitoredService _romMonitoredService;
        private readonly IGameService _gameService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEpisodeRefreshedService _romRefreshedService;
        private readonly IEventAggregator _eventAggregator;

        private readonly Logger _logger;

        public SeriesScannedHandler(IEpisodeMonitoredService episodeMonitoredService,
                                    IGameService seriesService,
                                    IManageCommandQueue commandQueueManager,
                                    IEpisodeRefreshedService episodeRefreshedService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _romMonitoredService = episodeMonitoredService;
            _gameService = seriesService;
            _commandQueueManager = commandQueueManager;
            _romRefreshedService = episodeRefreshedService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void HandleScanEvents(Game game)
        {
            var addOptions = game.AddOptions;

            if (addOptions == null)
            {
                _romRefreshedService.Search(game.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", game.Title);
            _romMonitoredService.SetEpisodeMonitoredStatus(game, addOptions);

            _eventAggregator.PublishEvent(new SeriesAddCompletedEvent(game));

            // If both options are enabled search for the whole game, which will only include monitored roms.
            // This way multiple searches for the same platform are skipped, though a platform that can't be upgraded may be
            // searched, but the logs will be more explicit.

            if (addOptions.SearchForMissingEpisodes && addOptions.SearchForCutoffUnmetEpisodes)
            {
                _commandQueueManager.Push(new GameSearchCommand(game.Id));
            }
            else
            {
                if (addOptions.SearchForMissingEpisodes)
                {
                    _commandQueueManager.Push(new MissingRomSearchCommand(game.Id));
                }

                if (addOptions.SearchForCutoffUnmetEpisodes)
                {
                    _commandQueueManager.Push(new CutoffUnmetRomSearchCommand(game.Id));
                }
            }

            game.AddOptions = null;
            _gameService.RemoveAddOptions(game);
        }

        public void Handle(SeriesScannedEvent message)
        {
            HandleScanEvents(message.Game);
        }

        public void Handle(SeriesScanSkippedEvent message)
        {
            HandleScanEvents(message.Game);
        }
    }
}
