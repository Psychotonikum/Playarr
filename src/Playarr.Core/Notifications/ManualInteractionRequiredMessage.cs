using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class ManualInteractionRequiredMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public RemoteEpisode Rom { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public QualityModel Quality { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
