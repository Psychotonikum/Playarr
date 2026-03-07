using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Download
{
    public class ProcessMonitoredDownloadsCommand : Command
    {
        public override bool RequiresDiskAccess => true;

        public override bool IsLongRunning => true;
    }
}
