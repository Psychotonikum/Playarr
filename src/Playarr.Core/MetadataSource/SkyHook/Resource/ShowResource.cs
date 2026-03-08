using System.Collections.Generic;

namespace Playarr.Core.MetadataSource.SkyHook.Resource
{
    public class ShowResource
    {
        public ShowResource()
        {
            Actors = new List<ActorResource>();
            Genres = new List<string>();
            Images = new List<ImageResource>();
            Platforms = new List<SeasonResource>();
            Roms = new List<RomResource>();
        }

        public int IgdbId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }

        // public string Language { get; set; }
        public string Slug { get; set; }
        public string FirstAired { get; set; }
        public string LastAired { get; set; }
        public int? MobyGamesId { get; set; }
        public int? RawgId { get; set; }
        public int? TmdbId { get; set; }
        public HashSet<int> MalIds { get; set; }
        public HashSet<int> AniListIds { get; set; }
        public string Status { get; set; }
        public int? Runtime { get; set; }
        public TimeOfDayResource TimeOfDay { get; set; }

        public string Network { get; set; }
        public string ImdbId { get; set; }
        public string OriginalLanguage { get; set; }
        public string OriginalCountry { get; set; }

        public List<ActorResource> Actors { get; set; }
        public List<string> Genres { get; set; }

        public string ContentRating { get; set; }

        public RatingResource Rating { get; set; }

        public List<ImageResource> Images { get; set; }
        public List<SeasonResource> Platforms { get; set; }
        public List<RomResource> Roms { get; set; }
    }
}
