using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Discord
{
    public class DiscordException : PlayarrException
    {
        public DiscordException(string message)
            : base(message)
        {
        }

        public DiscordException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
