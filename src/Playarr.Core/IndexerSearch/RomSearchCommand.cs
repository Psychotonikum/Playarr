using System.Collections.Generic;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class EpisodeSearchCommand : Command
    {
        public List<int> RomIds { get; set; }

        public override bool SendUpdatesToClient => true;

        public EpisodeSearchCommand()
        {
        }

        public EpisodeSearchCommand(List<int> romIds)
        {
            RomIds = romIds;
        }
    }
}
