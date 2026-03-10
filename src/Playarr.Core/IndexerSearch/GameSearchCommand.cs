using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class GameSearchCommand : Command
    {
        public int GameId { get; set; }

        public override bool SendUpdatesToClient => true;

        public GameSearchCommand()
        {
        }

        public GameSearchCommand(int gameId)
        {
            GameId = gameId;
        }
    }
}
