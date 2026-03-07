using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class MissingEpisodeSearchCommand : Command
    {
        public int? SeriesId { get; set; }
        public bool Monitored { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingEpisodeSearchCommand()
        {
            Monitored = true;
        }

        public MissingEpisodeSearchCommand(int gameId)
        {
            SeriesId = gameId;
            Monitored = true;
        }
    }
}
