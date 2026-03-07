using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Plex
{
    public class PlexException : PlayarrException
    {
        public PlexException(string message)
            : base(message)
        {
        }

        public PlexException(string message, params object[] args)
            : base(message, args)
        {
        }

        public PlexException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
