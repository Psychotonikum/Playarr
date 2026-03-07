using System.Net;
using Playarr.Core.Exceptions;

namespace Playarr.Core.MetadataSource.SkyHook;

public class InvalidSearchTermException : PlayarrClientException
{
    public InvalidSearchTermException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
