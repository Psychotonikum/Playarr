using Playarr.Common.Exceptions;

namespace Playarr.Common.Disk
{
    public class NotParentException : PlayarrException
    {
        public NotParentException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NotParentException(string message)
            : base(message)
        {
        }
    }
}
