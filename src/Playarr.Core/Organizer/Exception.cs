using Playarr.Common.Exceptions;

namespace Playarr.Core.Organizer
{
    public class NamingFormatException : PlayarrException
    {
        public NamingFormatException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NamingFormatException(string message)
            : base(message)
        {
        }
    }
}
