using System;
using System.Net;
using Playarr.Core.Exceptions;

namespace Playarr.Core.MetadataSource.SkyHook
{
    public class SkyHookException : PlayarrClientException
    {
        public SkyHookException(string message)
            : base(HttpStatusCode.ServiceUnavailable, message)
        {
        }

        public SkyHookException(string message, params object[] args)
            : base(HttpStatusCode.ServiceUnavailable, message, args)
        {
        }

        public SkyHookException(string message, Exception innerException, params object[] args)
            : base(HttpStatusCode.ServiceUnavailable, message, innerException, args)
        {
        }
    }
}
