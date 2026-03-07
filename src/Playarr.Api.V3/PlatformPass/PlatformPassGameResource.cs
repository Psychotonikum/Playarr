using System.Collections.Generic;
using Playarr.Api.V3.Game;

namespace Playarr.Api.V3.PlatformPass
{
    public class PlatformPassGameResource
    {
        public int Id { get; set; }
        public bool? Monitored { get; set; }
        public List<SeasonResource> Platforms { get; set; }
    }
}
