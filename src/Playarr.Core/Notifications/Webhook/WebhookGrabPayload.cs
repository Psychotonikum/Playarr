using System.Collections.Generic;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookGrabPayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookEpisode> Roms { get; set; }
        public WebhookRelease Release { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
        public WebhookCustomFormatInfo CustomFormatInfo { get; set; }
    }
}
