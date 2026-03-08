using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class SeasonSearchCommand : Command
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
