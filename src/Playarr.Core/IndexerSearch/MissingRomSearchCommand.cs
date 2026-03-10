using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class MissingRomSearchCommand : Command
    {
        public int? GameId { get; set; }
        public bool Monitored { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingRomSearchCommand()
        {
            Monitored = true;
        }

        public MissingRomSearchCommand(int gameId)
        {
            GameId = gameId;
            Monitored = true;
        }
    }
}
