using System.Collections.Generic;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch
{
    public class EpisodeSearchGroup
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public List<Rom> Roms { get; set; }
    }
}
