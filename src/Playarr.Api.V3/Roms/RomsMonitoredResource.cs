using System.Collections.Generic;

namespace Playarr.Api.V3.Roms
{
    public class EpisodesMonitoredResource
    {
        public List<int> RomIds { get; set; }
        public bool Monitored { get; set; }
    }
}
