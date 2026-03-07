using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Indexers
{
    public class RssSyncCommand : Command
    {
        public override bool SendUpdatesToClient => true;
        public override bool IsLongRunning => true;
    }
}
