using System;

namespace Playarr.Core.MetadataSource.SkyHook.Resource
{
    public class RomResource
    {
        public int TvdbId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? AiredAfterPlatformNumber { get; set; }
        public int? AiredBeforePlatformNumber { get; set; }
        public int? AiredBeforeRomNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int Runtime { get; set; }
        public string FinaleType { get; set; }
        public RatingResource Rating { get; set; }
        public string Overview { get; set; }
        public string Image { get; set; }
    }
}
