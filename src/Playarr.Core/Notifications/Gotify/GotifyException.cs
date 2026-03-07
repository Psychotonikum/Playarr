using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Gotify
{
    public class GotifyException : PlayarrException
    {
        public GotifyException(string message)
            : base(message)
        {
        }

        public GotifyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public GotifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
