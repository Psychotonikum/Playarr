using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Synology
{
    public class SynologyException : PlayarrException
    {
        public SynologyException(string message)
            : base(message)
        {
        }

        public SynologyException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
