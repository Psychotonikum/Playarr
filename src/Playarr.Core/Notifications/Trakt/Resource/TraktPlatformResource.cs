using System.Collections.Generic;

namespace Playarr.Core.Notifications.Trakt.Resource
{
    public class TraktSeasonResource
    {
        public int Number { get; set; }
        public List<TraktRomResource> Roms { get; set; }
    }
}
