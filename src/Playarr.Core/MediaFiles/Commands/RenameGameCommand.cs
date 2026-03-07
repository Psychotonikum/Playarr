using System.Collections.Generic;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.MediaFiles.Commands
{
    public class RenameSeriesCommand : Command
    {
        public List<int> GameIds { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameSeriesCommand()
        {
            GameIds = new List<int>();
        }
    }
}
