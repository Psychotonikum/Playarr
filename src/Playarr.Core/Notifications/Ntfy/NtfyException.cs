using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Ntfy
{
    public class NtfyException : PlayarrException
    {
        public NtfyException(string message)
            : base(message)
        {
        }

        public NtfyException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
