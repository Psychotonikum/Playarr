using System.Text.Json.Serialization;
using Playarr.Common.Extensions;
using Playarr.Core.Languages;
using Playarr.Core.MediaCover;
using Playarr.Core.Games;
using Playarr.Http.REST;
using Swashbuckle.AspNetCore.Annotations;

namespace Playarr.Api.V5.Game;

public class GameResource : RestResource
{
    public string? Title { get; set; }
    public List<AlternateTitleResource>? AlternateTitles { get; set; }
    public string? SortTitle { get; set; }
    public GameStatusType Status { get; set; }
    public bool Ended => Status == GameStatusType.Ended;
    public string? ProfileName { get; set; }
    public string? Overview { get; set; }
    public DateTime? NextAiring { get; set; }
    public DateTime? PreviousAiring { get; set; }
    public string? Network { get; set; }
    public string? AirTime { get; set; }
    public List<MediaCover>? Images { get; set; }
    public Language? OriginalLanguage { get; set; }
    public string? RemotePoster { get; set; }
    public List<SeasonResource> Platforms { get; set; } = new();
    public int Year { get; set; }
    public string? Path { get; set; }
    public int QualityProfileId { get; set; }
    public bool PlatformFolder { get; set; }
    public bool Monitored { get; set; }
    public NewItemMonitorTypes MonitorNewItems { get; set; }
    public bool UseSceneNumbering { get; set; }
    public int Runtime { get; set; }
    public int TvdbId { get; set; }
    public int MobyGamesId { get; set; }
    public int RawgId { get; set; }
    public int TmdbId { get; set; }
    public HashSet<int>? MalIds { get; set; }
    public HashSet<int>? AniListIds { get; set; }
    public DateTime? FirstAired { get; set; }
    public DateTime? LastAired { get; set; }
    public GameTypes SeriesType { get; set; }
    public string? CleanTitle { get; set; }
    public string? ImdbId { get; set; }
    public string? TitleSlug { get; set; }
    public string? RootFolderPath { get; set; }
    public string? Folder { get; set; }
    public string? Certification { get; set; }
    public List<string>? Genres { get; set; }
    public string? OriginalCountry { get; set; }
    public HashSet<int>? Tags { get; set; }
    public DateTime Added { get; set; }
    public AddGameOptions? AddOptions { get; set; }
    public Ratings? Ratings { get; set; }
    public SeriesStatisticsResource? Statistics { get; set; }
    public bool? EpisodesChanged { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [SwaggerIgnore]
    public bool? IsExcluded { get; set; }
}

public static class GameResourceMapper
{
    public static GameResource ToResource(this Playarr.Core.Games.Game model, bool includeSeasonImages = false)
    {
        return new GameResource
        {
            Id = model.Id,
            Title = model.Title,
            SortTitle = model.SortTitle,
            Status = model.Status,
            Overview = model.Overview,
            Network = model.Network,
            AirTime = model.AirTime,
            Images = model.Images.JsonClone(),
            Platforms = model.Platforms.ToResource(includeSeasonImages),
            Year = model.Year,
            OriginalCountry = model.OriginalCountry,
            OriginalLanguage = model.OriginalLanguage,
            Path = model.Path,
            QualityProfileId = model.QualityProfileId,
            PlatformFolder = model.PlatformFolder,
            Monitored = model.Monitored,
            MonitorNewItems = model.MonitorNewItems,
            UseSceneNumbering = model.UseSceneNumbering,
            Runtime = model.Runtime,
            TvdbId = model.TvdbId,
            MobyGamesId = model.MobyGamesId,
            RawgId = model.RawgId,
            TmdbId = model.TmdbId,
            MalIds = model.MalIds,
            AniListIds = model.AniListIds,
            FirstAired = model.FirstAired,
            LastAired = model.LastAired,
            SeriesType = model.SeriesType,
            CleanTitle = model.CleanTitle,
            ImdbId = model.ImdbId,
            TitleSlug = model.TitleSlug,
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
        return new Playarr.Core.Games.Game
        {
            Id = resource.Id,
            Title = resource.Title,
            SortTitle = resource.SortTitle,
            Status = resource.Status,
            Overview = resource.Overview,
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
            TvdbId = resource.TvdbId,
            MobyGamesId = resource.MobyGamesId,
            RawgId = resource.RawgId,
            TmdbId = resource.TmdbId,
            MalIds = resource.MalIds,
            AniListIds = resource.AniListIds,
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

    public static List<GameResource> ToResource(this IEnumerable<Playarr.Core.Games.Game> game,
        bool includeSeasonImages = false)
    {
        return game.Select(s => ToResource(s, includeSeasonImages)).ToList();
    }

    public static List<Playarr.Core.Games.Game> ToModel(this IEnumerable<GameResource> resources)
    {
        return resources.Select(ToModel).ToList();
    }
}
