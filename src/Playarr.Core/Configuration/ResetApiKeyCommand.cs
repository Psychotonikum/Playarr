using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Configuration
{
    public class ResetApiKeyCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
