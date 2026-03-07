using Playarr.Common.Exceptions;

namespace Playarr.Mono.Disk
{
    public class LinuxPermissionsException : PlayarrException
    {
        public LinuxPermissionsException(string message, params object[] args)
            : base(message, args)
        {
        }

        public LinuxPermissionsException(string message)
            : base(message)
        {
        }
    }
}
