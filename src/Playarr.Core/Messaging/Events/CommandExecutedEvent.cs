using Playarr.Common.Messaging;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Messaging.Events
{
    public class CommandExecutedEvent : IEvent
    {
        public CommandModel Command { get; private set; }

        public CommandExecutedEvent(CommandModel command)
        {
            Command = command;
        }
    }
}
