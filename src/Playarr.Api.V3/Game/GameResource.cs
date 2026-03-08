using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Languages;
using Playarr.Core.MediaCover;
using Playarr.Core.Games;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Game
{
    public class GameResource : RestResource
    {
        // Todo: Sorters should be done completely on the client
        // Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Game, History, Missing?
        // Todo: We should get the entire QualityProfile instead of ID and Name separately

        // View Only
        public string Title { get; set; }
        public List<AlternateTitleResource> AlternateTitles { get; set; }
        public string SortTitle { get; set; }

        // V3: replace with Ended
        public GameStatusType Status { get; set; }

        public bool Ended => Status == GameStatusType.Ended;

        public string ProfileName { get; set; }
        public string Overview { get; set; }
        public DateTime? NextAiring { get; set; }
        public DateTime? PreviousAiring { get; set; }
        public string Network { get; set; }
        public string AirTime { get; set; }
        public List<MediaCover> Images { get; set; }
        public Language OriginalLanguage { get; set; }
        public string RemotePoster { get; set; }
        public List<SeasonResource> Platforms { get; set; }
        public int Year { get; set; }

        // View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }

        // Editing Only
        public bool PlatformFolder { get; set; }
        public bool Monitored { get; set; }
        public NewItemMonitorTypes MonitorNewItems { get; set; }

        public bool UseSceneNumbering { get; set; }
        public int Runtime { get; set; }
        public int IgdbId { get; set; }
        public int MobyGamesId { get; set; }
        public int RawgId { get; set; }
        public int TmdbId { get; set; }
        public DateTime? FirstAired { get; set; }
        public DateTime? LastAired { get; set; }
        public GameTypes SeriesType { get; set; }
        public string CleanTitle { get; set; }
        public string ImdbId { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Folder { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddGameOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }

        public SeriesStatisticsResource Statistics { get; set; }

        public bool? EpisodesChanged { get; set; }

        [Obsolete("Deprecated")]
        public int LanguageProfileId  => 1;
    }

    public static class GameResourceMapper
    {
        public static GameResource ToResource(this Playarr.Core.Games.Game model, bool includeSeasonImages = false)
        {
            if (model == null)
            {
                return null;
            }

            return new GameResource
                   {
                       Id = model.Id,

                       Title = model.Title,

                       // AlternateTitles
                       SortTitle = model.SortTitle,

                       // TotalEpisodeCount
                       // EpisodeCount
                       // EpisodeFileCount
                       // SizeOnDisk
                       Status = model.Status,
                       Overview = model.Overview,

                       // NextAiring
                       // PreviousAiring
                       Network = model.Network,
                       AirTime = model.AirTime,

                       // JsonClone
                       Images = model.Images.JsonClone(),

                       Platforms = model.Platforms.ToResource(includeSeasonImages),
                       Year = model.Year,
                       OriginalLanguage = model.OriginalLanguage,

                       Path = model.Path,
                       QualityProfileId = model.QualityProfileId,

                       PlatformFolder = model.PlatformFolder,
                       Monitored = model.Monitored,
                       MonitorNewItems = model.MonitorNewItems,

                       UseSceneNumbering = model.UseSceneNumbering,
                       Runtime = model.Runtime,
                       IgdbId = model.IgdbId,
                       MobyGamesId = model.MobyGamesId,
                       RawgId = model.RawgId,
                       TmdbId = model.TmdbId,
                       FirstAired = model.FirstAired,
                       LastAired = model.LastAired,
                       SeriesType = model.SeriesType,
                       CleanTitle = model.CleanTitle,
                       ImdbId = model.ImdbId,
                       TitleSlug = model.TitleSlug,

                       // Root folder path needs to be calculated from the game path
                       // RootFolderPath = model.RootFolderPath,

                       Certification = model.Certification,
                       Genres = model.Genres,
                       Tags = model.Tags,
                       Added = model.Added,
                       AddOptions = model.AddOptions,
                       Ratings = model.Ratings
                   };
        }

        public static Playarr.Core.Games.Game ToModel(this GameResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Playarr.Core.Games.Game
                   {
                       Id = resource.Id,

                       Title = resource.Title,

                       // AlternateTitles
                       SortTitle = resource.SortTitle,

                       // TotalEpisodeCount
                       // EpisodeCount
                       // EpisodeFileCount
                       // SizeOnDisk
                       Status = resource.Status,
                       Overview = resource.Overview,

                       // NextAiring
                       // PreviousAiring
                       Network = resource.Network,
                       AirTime = resource.AirTime,
                       Images = resource.Images,

                       Platforms = resource.Platforms?.ToModel() ?? new List<Platform>(),
                       Year = resource.Year,
                       OriginalLanguage = resource.OriginalLanguage,

                       Path = resource.Path,
                       QualityProfileId = resource.QualityProfileId,

                       PlatformFolder = resource.PlatformFolder,
                       Monitored = resource.Monitored,
                       MonitorNewItems = resource.MonitorNewItems,

                       UseSceneNumbering = resource.UseSceneNumbering,
                       Runtime = resource.Runtime,
                       IgdbId = resource.IgdbId,
                       MobyGamesId = resource.MobyGamesId,
                       RawgId = resource.RawgId,
                       TmdbId = resource.TmdbId,
                       FirstAired = resource.FirstAired,
                       SeriesType = resource.SeriesType,
                       CleanTitle = resource.CleanTitle,
                       ImdbId = resource.ImdbId,
                       TitleSlug = resource.TitleSlug,
                       RootFolderPath = resource.RootFolderPath,
                       Certification = resource.Certification,
                       Genres = resource.Genres,
                       Tags = resource.Tags,
                       Added = resource.Added,
                       AddOptions = resource.AddOptions,
                       Ratings = resource.Ratings
                   };
        }

        public static Playarr.Core.Games.Game ToModel(this GameResource resource, Playarr.Core.Games.Game game)
        {
            var updatedSeries = resource.ToModel();

            game.ApplyChanges(updatedSeries);

            return game;
        }

        public static List<GameResource> ToResource(this IEnumerable<Playarr.Core.Games.Game> game, bool includeSeasonImages = false)
        {
            return game.Select(s => ToResource(s, includeSeasonImages)).ToList();
        }

        public static List<Playarr.Core.Games.Game> ToModel(this IEnumerable<GameResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
