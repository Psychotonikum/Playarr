using Playarr.Common.Exceptions;

namespace Playarr.Core.Messaging.Commands
{
    public class CommandNotFoundException : PlayarrException
    {
        public CommandNotFoundException(string contract)
            : base("Couldn't find command " + contract)
        {
        }
    }
}
