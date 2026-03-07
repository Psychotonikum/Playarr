using System;
using Playarr.Core.Datastore;

namespace Playarr.Core.Games
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }
        public MonitorTypes Monitor { get; set; }
    }

    public enum MonitorTypes
    {
        Unknown,
        All,
        Future,
        Missing,
        Existing,
        FirstSeason,
        LastSeason,

        [Obsolete]
        LatestSeason,

        Pilot,
        Recent,
        MonitorSpecials,
        UnmonitorSpecials,
        None,
        Skip
    }

    public enum NewItemMonitorTypes
    {
        All,
        None
    }
}
