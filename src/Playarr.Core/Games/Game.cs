using System;
using System.Collections.Generic;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Languages;
using Playarr.Core.Profiles.Qualities;

namespace Playarr.Core.Games
{
    public class Game : ModelBase
    {
        public Game()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Actors = new List<Actor>();
            Platforms = new List<Platform>();
            Tags = new HashSet<int>();
            OriginalLanguage = Language.English;
            MalIds = new HashSet<int>();
            AniListIds = new HashSet<int>();
        }

        public int IgdbId { get; set; }
        public int MobyGamesId { get; set; }
        public int RawgId { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public HashSet<int> MalIds { get; set; }
        public HashSet<int> AniListIds { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public GameStatusType Status { get; set; }
        public string Overview { get; set; }
        public string AirTime { get; set; }
        public bool Monitored { get; set; }
        public NewItemMonitorTypes MonitorNewItems { get; set; }
        public int QualityProfileId { get; set; }
        public bool PlatformFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public GameTypes SeriesType { get; set; }
        public string Network { get; set; }
        public bool UseSceneNumbering { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public List<Actor> Actors { get; set; }
        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public DateTime? FirstAired { get; set; }
        public DateTime? LastAired { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        public Language OriginalLanguage { get; set; }
        public string OriginalCountry { get; set; }
        public List<Platform> Platforms { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddGameOptions AddOptions { get; set; }
        public int? GameSystemId { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", IgdbId, Title.NullSafe());
        }

        public void ApplyChanges(Game otherGame)
        {
            IgdbId = otherGame.IgdbId;

            Platforms = otherGame.Platforms;
            Path = otherGame.Path;
            QualityProfileId = otherGame.QualityProfileId;

            PlatformFolder = otherGame.PlatformFolder;
            Monitored = otherGame.Monitored;
            MonitorNewItems = otherGame.MonitorNewItems;

            SeriesType = otherGame.SeriesType;
            RootFolderPath = otherGame.RootFolderPath;
            Tags = otherGame.Tags;
            AddOptions = otherGame.AddOptions;
        }
    }
}
