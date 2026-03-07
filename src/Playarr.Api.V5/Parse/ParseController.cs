using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.Download.Aggregation;
using Playarr.Core.Parser;
using Playarr.Api.V5.CustomFormats;
using Playarr.Api.V5.Roms;
using Playarr.Api.V5.Game;
using Playarr.Http;

namespace Playarr.Api.V5.Parse;

[V5ApiController]
public class ParseController : Controller
{
    private readonly IParsingService _parsingService;
    private readonly IRemoteEpisodeAggregationService _aggregationService;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public ParseController(IParsingService parsingService,
                           IRemoteEpisodeAggregationService aggregationService,
                           ICustomFormatCalculationService formatCalculator)
    {
        _parsingService = parsingService;
        _aggregationService = aggregationService;
        _formatCalculator = formatCalculator;
    }

    [HttpGet]
    [Produces("application/json")]
    public ParseResource Parse(string? title, string? path)
    {
        if (title.IsNullOrWhiteSpace())
        {
            return new ParseResource
            {
                Title = title
            };
        }

        var parsedRomInfo = path.IsNotNullOrWhiteSpace() ? Parser.ParsePath(path) : Parser.ParseTitle(title);

        if (parsedRomInfo == null)
        {
            return new ParseResource
            {
                Title = title
            };
        }

        var remoteRom = _parsingService.Map(parsedRomInfo, 0, 0, null);

        if (remoteRom != null)
        {
            _aggregationService.Augment(remoteRom);

            remoteRom.CustomFormats = _formatCalculator.ParseCustomFormat(remoteRom, 0);
            remoteRom.CustomFormatScore = remoteRom.Game?.QualityProfile?.Value.CalculateCustomFormatScore(remoteRom.CustomFormats) ?? 0;

            return new ParseResource
            {
                Title = title,
                ParsedRomInfo = remoteRom.ParsedRomInfo,
                Game = remoteRom.Game?.ToResource(),
                Roms = remoteRom.Roms.ToResource(),
                Languages = remoteRom.Languages,
                CustomFormats = remoteRom.CustomFormats?.ToResource(false),
                CustomFormatScore = remoteRom.CustomFormatScore
            };
        }
        else
        {
            return new ParseResource
            {
                Title = title,
                ParsedRomInfo = parsedRomInfo
            };
        }
    }
}
