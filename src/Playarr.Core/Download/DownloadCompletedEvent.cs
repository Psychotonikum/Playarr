using System.Collections.Generic;
using Playarr.Common.Messaging;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download
{
    public class DownloadCompletedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }
        public int GameId { get; private set; }
        public List<RomFile> RomFiles { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public DownloadCompletedEvent(TrackedDownload trackedDownload, int gameId, List<RomFile> romFiles, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            GameId = gameId;
            RomFiles = romFiles;
            Release = release;
        }
    }
}
