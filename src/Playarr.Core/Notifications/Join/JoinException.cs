using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Join
{
    public class JoinException : PlayarrException
    {
        public JoinException(string message)
            : base(message)
        {
        }

        public JoinException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
