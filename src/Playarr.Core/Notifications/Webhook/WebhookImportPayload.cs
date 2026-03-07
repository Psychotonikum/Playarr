using System.Collections.Generic;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookImportPayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookEpisode> Roms { get; set; }
        public WebhookRomFile RomFile { get; set; }
        public bool IsUpgrade { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
        public List<WebhookRomFile> DeletedFiles { get; set; }
        public WebhookCustomFormatInfo CustomFormatInfo { get; set; }
        public WebhookGrabbedRelease Release { get; set; }
    }
}
