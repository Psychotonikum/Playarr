using System.Collections.Generic;
using Playarr.Core.MediaFiles;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookEpisodeDeletePayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookEpisode> Roms { get; set; }
        public WebhookRomFile RomFile { get; set; }
        public DeleteMediaFileReason DeleteReason { get; set; }
    }
}
