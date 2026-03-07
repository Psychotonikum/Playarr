using Playarr.Common.Messaging;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.ProgressMessaging
{
    public class CommandUpdatedEvent : IEvent
    {
        public CommandModel Command { get; set; }

        public CommandUpdatedEvent(CommandModel command)
        {
            Command = command;
        }
    }
}
