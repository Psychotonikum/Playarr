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
        private readonly IEpisodeMonitoredService _episodeMonitoredService;
        private readonly IGameService _seriesService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEpisodeRefreshedService _episodeRefreshedService;
        private readonly IEventAggregator _eventAggregator;

        private readonly Logger _logger;

        public SeriesScannedHandler(IEpisodeMonitoredService episodeMonitoredService,
                                    IGameService seriesService,
                                    IManageCommandQueue commandQueueManager,
                                    IEpisodeRefreshedService episodeRefreshedService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _episodeMonitoredService = episodeMonitoredService;
            _seriesService = seriesService;
            _commandQueueManager = commandQueueManager;
            _episodeRefreshedService = episodeRefreshedService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void HandleScanEvents(Game game)
        {
            var addOptions = game.AddOptions;

            if (addOptions == null)
            {
                _episodeRefreshedService.Search(game.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", game.Title);
            _episodeMonitoredService.SetEpisodeMonitoredStatus(game, addOptions);

            _eventAggregator.PublishEvent(new SeriesAddCompletedEvent(game));

            // If both options are enabled search for the whole game, which will only include monitored roms.
            // This way multiple searches for the same platform are skipped, though a platform that can't be upgraded may be
            // searched, but the logs will be more explicit.

            if (addOptions.SearchForMissingEpisodes && addOptions.SearchForCutoffUnmetEpisodes)
            {
                _commandQueueManager.Push(new SeriesSearchCommand(game.Id));
            }
            else
            {
                if (addOptions.SearchForMissingEpisodes)
                {
                    _commandQueueManager.Push(new MissingEpisodeSearchCommand(game.Id));
                }

                if (addOptions.SearchForCutoffUnmetEpisodes)
                {
                    _commandQueueManager.Push(new CutoffUnmetEpisodeSearchCommand(game.Id));
                }
            }

            game.AddOptions = null;
            _seriesService.RemoveAddOptions(game);
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
