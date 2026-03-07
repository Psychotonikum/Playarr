using System.Collections.Generic;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookRenamePayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public List<WebhookRenamedRomFile> RenamedRomFiles { get; set; }
    }
}
