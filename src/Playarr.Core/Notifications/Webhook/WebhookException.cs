using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookException : PlayarrException
    {
        public WebhookException(string message)
            : base(message)
        {
        }

        public WebhookException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
