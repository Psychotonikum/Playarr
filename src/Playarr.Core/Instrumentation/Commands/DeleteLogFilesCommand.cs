using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Instrumentation.Commands
{
    public class DeleteLogFilesCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
