using System;

namespace Playarr.Common.Exceptions
{
    public abstract class PlayarrException : ApplicationException
    {
        protected PlayarrException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        protected PlayarrException(string message)
            : base(message)
        {
        }

        protected PlayarrException(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }

        protected PlayarrException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
