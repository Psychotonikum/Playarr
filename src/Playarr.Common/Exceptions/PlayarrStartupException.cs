using System;

namespace Playarr.Common.Exceptions
{
    public class PlayarrStartupException : PlayarrException
    {
        public PlayarrStartupException(string message, params object[] args)
            : base("Playarr failed to start: " + string.Format(message, args))
        {
        }

        public PlayarrStartupException(string message)
            : base("Playarr failed to start: " + message)
        {
        }

        public PlayarrStartupException()
            : base("Playarr failed to start")
        {
        }

        public PlayarrStartupException(Exception innerException, string message, params object[] args)
            : base("Playarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public PlayarrStartupException(Exception innerException, string message)
            : base("Playarr failed to start: " + message, innerException)
        {
        }

        public PlayarrStartupException(Exception innerException)
            : base("Playarr failed to start: " + innerException.Message)
        {
        }
    }
}
