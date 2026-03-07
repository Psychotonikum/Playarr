using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Notifiarr
{
    public class NotifiarrException : PlayarrException
    {
        public NotifiarrException(string message)
            : base(message)
        {
        }

        public NotifiarrException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
