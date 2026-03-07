using Playarr.Common.Exceptions;

namespace Playarr.Core.Parser
{
    public class InvalidSeasonException : PlayarrException
    {
        public InvalidSeasonException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidSeasonException(string message)
            : base(message)
        {
        }
    }
}
