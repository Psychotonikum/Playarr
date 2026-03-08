using Playarr.Core.CustomFormats;
using Playarr.Core.History;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Api.V5.CustomFormats;
using Playarr.Api.V5.Roms;
using Playarr.Api.V5.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V5.History;

public class HistoryResource : RestResource
{
    public int EpisodeId { get; set; }
    public int GameId { get; set; }
    public required string SourceTitle { get; set; }
    public required List<Language> Languages { get; set; }
    public required QualityModel Quality { get; set; }
    public required List<CustomFormatResource> CustomFormats { get; set; }
    public int CustomFormatScore { get; set; }
    public bool QualityCutoffNotMet { get; set; }
    public DateTime Date { get; set; }
    public string? DownloadId { get; set; }
    public EpisodeHistoryEventType EventType { get; set; }
    public required Dictionary<string, string> Data { get; set; }
    public RomResource? Rom { get; set; }
    public GameResource? Game { get; set; }
}

public static class HistoryResourceMapper
{
    public static HistoryResource ToResource(this EpisodeHistory model, ICustomFormatCalculationService formatCalculator)
    {
        var customFormats = formatCalculator.ParseCustomFormat(model, model.Game);
        var customFormatScore = model.Game.QualityProfile.Value.CalculateCustomFormatScore(customFormats);

        return new HistoryResource
        {
            Id = model.Id,
            EpisodeId = model.EpisodeId,
            GameId = model.GameId,
            SourceTitle = model.SourceTitle,
            Languages = model.Languages,
            Quality = model.Quality,
            CustomFormats = customFormats.ToResource(false),
            CustomFormatScore = customFormatScore,
            Date = model.Date,
            DownloadId = model.DownloadId,
            EventType = model.EventType,
            Data = model.Data
        };
    }
}
