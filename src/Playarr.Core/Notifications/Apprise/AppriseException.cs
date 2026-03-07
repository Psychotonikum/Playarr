using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Apprise
{
    public class AppriseException : PlayarrException
    {
        public AppriseException(string message)
            : base(message)
        {
        }

        public AppriseException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
