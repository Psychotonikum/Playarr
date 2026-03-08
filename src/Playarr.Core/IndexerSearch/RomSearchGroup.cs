using System.Collections.Generic;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch
{
    public class EpisodeSearchGroup
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public List<Rom> Roms { get; set; }
    }
}
