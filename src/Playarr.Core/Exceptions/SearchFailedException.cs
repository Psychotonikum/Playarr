using Playarr.Common.Exceptions;

namespace Playarr.Core.Exceptions
{
    public class SearchFailedException : PlayarrException
    {
        public SearchFailedException(string message)
            : base(message)
        {
        }
    }
}
