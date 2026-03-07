using System.Linq;
using NLog;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Download
{
    public interface IIgnoredDownloadService
    {
        bool IgnoreDownload(TrackedDownload trackedDownload);
    }

    public class IgnoredDownloadService : IIgnoredDownloadService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public IgnoredDownloadService(IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool IgnoreDownload(TrackedDownload trackedDownload)
        {
            var game = trackedDownload.RemoteEpisode.Game;

            if (game == null)
            {
                _logger.Warn("Unable to ignore download for unknown game");
                return false;
            }

            var roms = trackedDownload.RemoteEpisode.Roms;

            var downloadIgnoredEvent = new DownloadIgnoredEvent
                                      {
                                          SeriesId = game.Id,
                                          RomIds = roms.Select(e => e.Id).ToList(),
                                          Languages = trackedDownload.RemoteEpisode.Languages,
                                          Quality = trackedDownload.RemoteEpisode.ParsedRomInfo.Quality,
                                          SourceTitle = trackedDownload.DownloadItem.Title,
                                          DownloadClientInfo = trackedDownload.DownloadItem.DownloadClientInfo,
                                          DownloadId = trackedDownload.DownloadItem.DownloadId,
                                          TrackedDownload = trackedDownload,
                                          Message = "Manually ignored"
                                      };

            _eventAggregator.PublishEvent(downloadIgnoredEvent);
            return true;
        }
    }
}
