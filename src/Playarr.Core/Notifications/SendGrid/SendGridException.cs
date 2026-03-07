using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.SendGrid
{
    public class SendGridException : PlayarrException
    {
        public SendGridException(string message)
            : base(message)
        {
        }

        public SendGridException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
