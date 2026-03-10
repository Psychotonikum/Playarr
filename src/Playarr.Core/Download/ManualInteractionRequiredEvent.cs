using Playarr.Common.Messaging;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download
{
    public class ManualInteractionRequiredEvent : IEvent
    {
        public RemoteRom Rom { get; private set; }
        public TrackedDownload TrackedDownload { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public ManualInteractionRequiredEvent(TrackedDownload trackedDownload, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            Rom = trackedDownload.RemoteRom;
            Release = release;
        }
    }
}
