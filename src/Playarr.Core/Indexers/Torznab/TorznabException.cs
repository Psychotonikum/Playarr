using Playarr.Common.Exceptions;

namespace Playarr.Core.Indexers.Torznab
{
    public class TorznabException : PlayarrException
    {
        public TorznabException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TorznabException(string message)
            : base(message)
        {
        }
    }
}
