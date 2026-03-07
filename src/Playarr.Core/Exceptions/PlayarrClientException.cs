using System;
using System.Net;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Exceptions
{
    public class PlayarrClientException : PlayarrException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public PlayarrClientException(HttpStatusCode statusCode, string message, params object[] args)
            : base(message, args)
        {
            StatusCode = statusCode;
        }

        public PlayarrClientException(HttpStatusCode statusCode, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
            StatusCode = statusCode;
        }

        public PlayarrClientException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
