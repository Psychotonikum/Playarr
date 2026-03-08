using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.MediaFiles.Commands
{
    public class RescanSeriesCommand : Command
    {
        public int? GameId { get; set; }

        public override bool SendUpdatesToClient => true;

        public RescanSeriesCommand()
        {
        }

        public RescanSeriesCommand(int gameId)
        {
            GameId = gameId;
        }
    }
}
