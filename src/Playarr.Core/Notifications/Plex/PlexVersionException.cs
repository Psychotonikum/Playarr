using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Plex
{
    public class PlexVersionException : PlayarrException
    {
        public PlexVersionException(string message)
            : base(message)
        {
        }

        public PlexVersionException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
