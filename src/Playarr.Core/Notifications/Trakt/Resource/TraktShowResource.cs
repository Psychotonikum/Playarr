using System.Collections.Generic;
namespace Playarr.Core.Notifications.Trakt.Resource
{
    public class TraktShowResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public TraktShowIdsResource Ids { get; set; }
        public List<TraktSeasonResource> Platforms { get; set; }
    }
}
