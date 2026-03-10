using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class CutoffUnmetRomSearchCommand : Command
    {
        public int? GameId { get; set; }
        public bool Monitored { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public CutoffUnmetRomSearchCommand()
        {
            Monitored = true;
        }

        public CutoffUnmetRomSearchCommand(int gameId)
        {
            GameId = gameId;
            Monitored = true;
        }
    }
}
