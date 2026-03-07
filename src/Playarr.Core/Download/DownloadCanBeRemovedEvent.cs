using Playarr.Common.Messaging;
using Playarr.Core.Download.TrackedDownloads;

namespace Playarr.Core.Download
{
    public class DownloadCanBeRemovedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public DownloadCanBeRemovedEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
