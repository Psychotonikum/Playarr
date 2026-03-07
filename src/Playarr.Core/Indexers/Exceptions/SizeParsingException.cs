using Playarr.Common.Exceptions;

namespace Playarr.Core.Indexers.Exceptions
{
    public class SizeParsingException : PlayarrException
    {
        public SizeParsingException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
