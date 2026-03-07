using System.Collections.Generic;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookImportCompletePayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookEpisode> Roms { get; set; }
        public List<WebhookRomFile> RomFiles { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
        public WebhookGrabbedRelease Release { get; set; }
        public int FileCount => RomFiles.Count;
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
    }
}
