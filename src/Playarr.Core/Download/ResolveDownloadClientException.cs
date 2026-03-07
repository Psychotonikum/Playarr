using System.Net;
using Playarr.Core.Exceptions;

namespace Playarr.Core.Download;

public class ResolveDownloadClientException : PlayarrClientException
{
    public ResolveDownloadClientException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
