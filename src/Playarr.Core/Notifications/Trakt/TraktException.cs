using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Trakt
{
    public class TraktException : PlayarrException
    {
        public TraktException(string message)
            : base(message)
        {
        }

        public TraktException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
