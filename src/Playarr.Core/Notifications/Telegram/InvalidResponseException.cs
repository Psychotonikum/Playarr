using System;

namespace Playarr.Core.Notifications.Telegram
{
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message)
            : base(message)
        {
        }
    }
}
