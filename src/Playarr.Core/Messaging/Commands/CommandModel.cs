using System;
using Playarr.Common.Messaging;
using Playarr.Core.Datastore;

namespace Playarr.Core.Messaging.Commands
{
    public class CommandModel : ModelBase, IMessage
    {
        public string Name { get; set; }
        public Command Body { get; set; }
        public CommandPriority Priority { get; set; }
        public CommandStatus Status { get; set; }
        public CommandResult Result { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Exception { get; set; }
        public CommandTrigger Trigger { get; set; }
        public string Message { get; set; }
    }
}
