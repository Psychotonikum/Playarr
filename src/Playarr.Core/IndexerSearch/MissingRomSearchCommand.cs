using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class MissingEpisodeSearchCommand : Command
    {
        public int? GameId { get; set; }
        public bool Monitored { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingEpisodeSearchCommand()
        {
            Monitored = true;
        }

        public MissingEpisodeSearchCommand(int gameId)
        {
            GameId = gameId;
            Monitored = true;
        }
    }
}
