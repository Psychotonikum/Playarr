using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Blocklisting
{
    public class ClearBlocklistCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
