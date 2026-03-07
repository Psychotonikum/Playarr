using Playarr.Core.CustomFormats;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Api.V5.CustomFormats;
using Playarr.Api.V5.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Blocklist;

public class BlocklistResource : RestResource
{
    public int SeriesId { get; set; }
    public required List<int> RomIds { get; set; }
    public required string SourceTitle { get; set; }
    public required List<Language> Languages { get; set; }
    public required QualityModel Quality { get; set; }
    public required List<CustomFormatResource> CustomFormats { get; set; }
    public DateTime Date { get; set; }
    public DownloadProtocol Protocol { get; set; }
    public string? Indexer { get; set; }
    public string? Message { get; set; }
    public string? Source { get; set; }

    public required GameResource Game { get; set; }
}

public static class BlocklistResourceMapper
{
    public static BlocklistResource MapToResource(this Playarr.Core.Blocklisting.Blocklist model, ICustomFormatCalculationService formatCalculator)
    {
        return new BlocklistResource
        {
            Id = model.Id,
            SeriesId = model.SeriesId,
            RomIds = model.RomIds,
            SourceTitle = model.SourceTitle,
            Languages = model.Languages,
            Quality = model.Quality,
            CustomFormats = formatCalculator.ParseCustomFormat(model, model.Game).ToResource(false),
            Date = model.Date,
            Protocol = model.Protocol,
            Indexer = model.Indexer,
            Message = model.Message,
            Source = model.Source,
            Game = model.Game.ToResource()
        };
    }
}
