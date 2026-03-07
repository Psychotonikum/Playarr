using Playarr.Common.Exceptions;

namespace Playarr.Core.Indexers.Exceptions
{
    public class UnsupportedFeedException : PlayarrException
    {
        public UnsupportedFeedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UnsupportedFeedException(string message)
            : base(message)
        {
        }
    }
}
