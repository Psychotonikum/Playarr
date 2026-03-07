using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class SeriesSearchCommand : Command
    {
        public int SeriesId { get; set; }

        public override bool SendUpdatesToClient => true;

        public SeriesSearchCommand()
        {
        }

        public SeriesSearchCommand(int gameId)
        {
            SeriesId = gameId;
        }
    }
}
