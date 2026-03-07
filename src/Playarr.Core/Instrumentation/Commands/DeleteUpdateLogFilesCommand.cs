using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Instrumentation.Commands
{
    public class DeleteUpdateLogFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
