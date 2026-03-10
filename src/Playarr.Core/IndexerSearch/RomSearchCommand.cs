using System.Collections.Generic;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class RomSearchCommand : Command
    {
        public List<int> RomIds { get; set; }

        public override bool SendUpdatesToClient => true;

        public RomSearchCommand()
        {
        }

        public RomSearchCommand(List<int> romIds)
        {
            RomIds = romIds;
        }
    }
}
