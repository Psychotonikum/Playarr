using System.Net;
using Playarr.Core.Exceptions;

namespace Playarr.Core.Backup
{
    public class RestoreBackupFailedException : PlayarrClientException
    {
        public RestoreBackupFailedException(HttpStatusCode statusCode, string message, params object[] args)
            : base(statusCode, message, args)
        {
        }

        public RestoreBackupFailedException(HttpStatusCode statusCode, string message)
            : base(statusCode, message)
        {
        }
    }
}
