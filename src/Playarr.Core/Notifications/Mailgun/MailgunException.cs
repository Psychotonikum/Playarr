using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Mailgun
{
    public class MailgunException : PlayarrException
    {
        public MailgunException(string message)
            : base(message)
        {
        }

        public MailgunException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
