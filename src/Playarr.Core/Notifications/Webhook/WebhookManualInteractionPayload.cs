using System.Collections.Generic;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookManualInteractionPayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookEpisode> Roms { get; set; }
        public WebhookDownloadClientItem DownloadInfo { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
        public string DownloadStatus { get; set; }
        public List<WebhookDownloadStatusMessage> DownloadStatusMessages { get; set; }
        public WebhookCustomFormatInfo CustomFormatInfo { get; set; }
        public WebhookGrabbedRelease Release { get; set; }
    }
}
