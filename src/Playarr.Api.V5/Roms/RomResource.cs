using System.Text.Json.Serialization;
using Playarr.Core.MediaCover;
using Playarr.Core.Games;
using Playarr.Api.V5.RomFiles;
using Playarr.Api.V5.Game;
using Playarr.Http.REST;
using Swashbuckle.AspNetCore.Annotations;

namespace Playarr.Api.V5.Roms
{
    public class RomResource : RestResource
    {
        public int GameId { get; set; }
        public int IgdbId { get; set; }
        public int EpisodeFileId { get; set; }
        public int PlatformNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }
        public string? AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public int Runtime { get; set; }
        public string? FinaleType { get; set; }
        public string? Overview { get; set; }
        public RomFileResource? RomFile { get; set; }
        public bool HasFile { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? SceneAbsoluteEpisodeNumber { get; set; }
        public int? SceneEpisodeNumber { get; set; }
        public int? ScenePlatformNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? GrabDate { get; set; }
        public GameResource? Game { get; set; }
        public List<MediaCover>? Images { get; set; }

        // Hiding this so people don't think its usable (only used to set the initial state)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [SwaggerIgnore]
        public bool Grabbed { get; set; }
    }

    public static class RomResourceMapper
    {
        public static RomResource ToResource(this Rom model)
        {
            return new RomResource
            {
                Id = model.Id,

                GameId = model.GameId,
                IgdbId = model.IgdbId,
                EpisodeFileId = model.EpisodeFileId,
                PlatformNumber = model.PlatformNumber,
                EpisodeNumber = model.EpisodeNumber,
                Title = model.Title,
                AirDate = model.AirDate,
                AirDateUtc = model.AirDateUtc,
                Runtime = model.Runtime,
                FinaleType = model.FinaleType,
                Overview = model.Overview,
                LastSearchTime = model.LastSearchTime,

                // RomFile

                HasFile = model.HasFile,
                Monitored = model.Monitored,
                AbsoluteEpisodeNumber = model.AbsoluteEpisodeNumber,
                SceneAbsoluteEpisodeNumber = model.SceneAbsoluteEpisodeNumber,
                SceneEpisodeNumber = model.SceneEpisodeNumber,
                ScenePlatformNumber = model.ScenePlatformNumber,
                UnverifiedSceneNumbering = model.UnverifiedSceneNumbering,

                // Game = model.Game.MapToResource(),
            };
        }

        public static List<RomResource> ToResource(this IEnumerable<Rom> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
