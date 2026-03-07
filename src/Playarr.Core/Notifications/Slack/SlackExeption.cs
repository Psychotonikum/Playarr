using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.Slack
{
    public class SlackExeption : PlayarrException
    {
        public SlackExeption(string message)
            : base(message)
        {
        }

        public SlackExeption(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
