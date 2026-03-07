using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Prowl
{
    public class ProwlException : PlayarrException
    {
        public ProwlException(string message)
            : base(message)
        {
        }

        public ProwlException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
