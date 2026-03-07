namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookSeriesDeletePayload : WebhookPayload
    {
        public WebhookSeries Game { get; set; }
        public bool DeletedFiles { get; set; }
    }
}
