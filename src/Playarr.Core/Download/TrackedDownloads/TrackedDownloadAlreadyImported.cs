using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.History;

namespace Playarr.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadAlreadyImported
    {
        bool IsImported(TrackedDownload trackedDownload, List<EpisodeHistory> historyItems);
    }

    public class TrackedDownloadAlreadyImported : ITrackedDownloadAlreadyImported
    {
        private readonly Logger _logger;

        public TrackedDownloadAlreadyImported(Logger logger)
        {
            _logger = logger;
        }

        public bool IsImported(TrackedDownload trackedDownload, List<EpisodeHistory> historyItems)
        {
            _logger.Trace("Checking if all roms for '{0}' have been imported", trackedDownload.DownloadItem.Title);

            if (historyItems.Empty())
            {
                _logger.Trace("No history for {0}", trackedDownload.DownloadItem.Title);
                return false;
            }

            var allEpisodesImportedInHistory = trackedDownload.RemoteEpisode.Roms.All(e =>
            {
                var lastHistoryItem = historyItems.FirstOrDefault(h => h.EpisodeId == e.Id);

                if (lastHistoryItem == null)
                {
                    _logger.Trace("No history for rom: S{0:00}E{1:00} [{2}]", e.PlatformNumber, e.EpisodeNumber, e.Id);
                    return false;
                }

                _logger.Trace("Last event for rom: S{0:00}E{1:00} [{2}] is: {3}", e.PlatformNumber, e.EpisodeNumber, e.Id, lastHistoryItem.EventType);

                return lastHistoryItem.EventType == EpisodeHistoryEventType.DownloadFolderImported;
            });

            _logger.Trace("All roms for '{0}' have been imported: {1}", trackedDownload.DownloadItem.Title, allEpisodesImportedInHistory);

            return allEpisodesImportedInHistory;
        }
    }
}
