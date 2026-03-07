using Playarr.Common.Exceptions;

namespace Playarr.Core.Parser
{
    public class InvalidDateException : PlayarrException
    {
        public InvalidDateException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidDateException(string message)
            : base(message)
        {
        }
    }
}
