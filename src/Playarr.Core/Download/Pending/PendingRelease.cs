using System;
using Playarr.Core.Datastore;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public int SeriesId { get; set; }
        public string Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedRomInfo ParsedRomInfo { get; set; }
        public ReleaseInfo Release { get; set; }
        public PendingReleaseReason Reason { get; set; }
        public PendingReleaseAdditionalInfo AdditionalInfo { get; set; }

        // Not persisted
        public RemoteEpisode RemoteEpisode { get; set; }
    }

    public class PendingReleaseAdditionalInfo
    {
        public SeriesMatchType SeriesMatchType { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }
    }
}
