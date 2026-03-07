using System.Collections.Generic;
using Playarr.Common.Messaging;

namespace Playarr.Core.Download.TrackedDownloads
{
    public class TrackedDownloadsRemovedEvent : IEvent
    {
        public List<TrackedDownload> TrackedDownloads { get; private set; }

        public TrackedDownloadsRemovedEvent(List<TrackedDownload> trackedDownloads)
        {
            TrackedDownloads = trackedDownloads;
        }
    }
}
