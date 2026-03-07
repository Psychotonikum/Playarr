using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Instrumentation.Commands
{
    public class ClearLogCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
