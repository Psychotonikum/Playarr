using System.Net;
using Playarr.Core.Exceptions;

namespace Playarr.Core.Indexers;

public class ResolveIndexerException : PlayarrClientException
{
    public ResolveIndexerException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
