using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Datastore;

namespace Playarr.Core.Tags
{
    public class TagDetails : ModelBase
    {
        public string Label { get; set; }
        public List<int> GameIds { get; set; }
        public List<int> NotificationIds { get; set; }
        public List<int> RestrictionIds { get; set; }
        public List<int> ExcludedReleaseProfileIds { get; set; }
        public List<int> DelayProfileIds { get; set; }
        public List<int> ImportListIds { get; set; }
        public List<int> IndexerIds { get; set; }
        public List<int> AutoTagIds { get; set; }
        public List<int> DownloadClientIds { get; set; }

        public bool InUse => GameIds.Any() ||
                             NotificationIds.Any() ||
                             RestrictionIds.Any() ||
                             ExcludedReleaseProfileIds.Any() ||
                             DelayProfileIds.Any() ||
                             ImportListIds.Any() ||
                             IndexerIds.Any() ||
                             AutoTagIds.Any() ||
                             DownloadClientIds.Any();
    }
}
