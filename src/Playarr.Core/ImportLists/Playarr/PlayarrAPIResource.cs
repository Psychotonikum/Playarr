using System.Collections.Generic;

namespace Playarr.Core.ImportLists.Playarr
{
    public class PlayarrSeries
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public int TvdbId { get; set; }
        public string Overview { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public int Year { get; set; }
        public string TitleSlug { get; set; }
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public List<PlayarrSeason> Platforms { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public class PlayarrProfile
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class PlayarrTag
    {
        public string Label { get; set; }
        public int Id { get; set; }
    }

    public class PlayarrRootFolder
    {
        public string Path { get; set; }
        public int Id { get; set; }
    }

    public class PlayarrSeason
    {
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
    }
}
