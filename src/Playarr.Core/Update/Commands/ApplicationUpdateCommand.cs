using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Update.Commands
{
    public class ApplicationUpdateCommand : Command
    {
        public bool InstallMajorUpdate { get; set; }
        public override bool SendUpdatesToClient => true;
        public override bool IsExclusive => true;
    }
}
