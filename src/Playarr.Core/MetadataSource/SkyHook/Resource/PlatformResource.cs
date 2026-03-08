using System.Collections.Generic;

namespace Playarr.Core.MetadataSource.SkyHook.Resource
{
    public class SeasonResource
    {
        public SeasonResource()
        {
            Images = new List<ImageResource>();
        }

        public int PlatformNumber { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}
