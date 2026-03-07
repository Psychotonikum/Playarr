using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.IndexerSearch
{
    public class SeasonSearchCommand : Command
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
