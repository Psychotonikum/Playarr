using Playarr.Common.Exceptions;

namespace Playarr.Core.ImportLists.Exceptions
{
    public class ImportListException : PlayarrException
    {
        private readonly ImportListResponse _importListResponse;

        public ImportListException(ImportListResponse response, string message, params object[] args)
            : base(message, args)
        {
            _importListResponse = response;
        }

        public ImportListException(ImportListResponse response, string message)
            : base(message)
        {
            _importListResponse = response;
        }

        public ImportListResponse Response => _importListResponse;
    }
}
