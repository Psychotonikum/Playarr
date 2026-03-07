using System.Collections.Generic;
using Playarr.Core.Games;

namespace Playarr.Api.V3.PlatformPass
{
    public class PlatformPassResource
    {
        public List<PlatformPassGameResource> Game { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
