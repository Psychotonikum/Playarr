using Playarr.Common.Exceptions;

namespace Playarr.Core.Indexers.Exceptions
{
    public class ApiKeyException : PlayarrException
    {
        public ApiKeyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ApiKeyException(string message)
            : base(message)
        {
        }
    }
}
