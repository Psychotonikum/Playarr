using System.Collections.Generic;
using Playarr.Common.Messaging;

namespace Playarr.Core.Download.TrackedDownloads
{
    public class TrackedDownloadRefreshedEvent : IEvent
    {
        public List<TrackedDownload> TrackedDownloads { get; private set; }

        public TrackedDownloadRefreshedEvent(List<TrackedDownload> trackedDownloads)
        {
            TrackedDownloads = trackedDownloads;
        }
    }
}
