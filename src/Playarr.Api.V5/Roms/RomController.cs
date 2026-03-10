using Microsoft.AspNetCore.Mvc;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;
using Playarr.Http.REST.Attributes;

namespace Playarr.Api.V5.Roms;

[V5ApiController]
public class RomController : RomControllerWithSignalR
{
    public RomController(IGameService seriesService,
                         IRomService episodeService,
                         IUpgradableSpecification upgradableSpecification,
                         ICustomFormatCalculationService formatCalculator,
                         IBroadcastSignalRMessage signalRBroadcaster)
        : base(episodeService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
    {
    }

    [HttpGet]
    [Produces("application/json")]
    public List<RomResource> GetRoms(int? gameId, int? platformNumber, [FromQuery]List<int> romIds, int? romFileId, [FromQuery] EpisodeSubresource[]? includeSubresources = null)
    {
        var includeSeries = includeSubresources.Contains(EpisodeSubresource.Game);
        var includeRomFile = includeSubresources.Contains(EpisodeSubresource.RomFile);
        var includeImages = includeSubresources.Contains(EpisodeSubresource.Images);

        if (gameId.HasValue)
        {
            if (platformNumber.HasValue)
            {
                return MapToResource(_romService.GetRomsByPlatform(gameId.Value, platformNumber.Value), includeSeries, includeRomFile, includeImages);
            }

            return MapToResource(_romService.GetEpisodeBySeries(gameId.Value), includeSeries, includeRomFile, includeImages);
        }
        else if (romIds.Any())
        {
            return MapToResource(_romService.GetRoms(romIds), includeSeries, includeRomFile, includeImages);
        }
        else if (romFileId.HasValue)
        {
            return MapToResource(_romService.GetRomsByFileId(romFileId.Value), includeSeries, includeRomFile, includeImages);
        }

        throw new BadRequestException("gameId or romIds must be provided");
    }

    [RestPutById]
    [Consumes("application/json")]
    public ActionResult<RomResource> SetEpisodeMonitored([FromRoute] int id, [FromBody] RomResource resource)
    {
        _romService.SetEpisodeMonitored(id, resource.Monitored);

        resource = MapToResource(_romService.GetEpisode(id), false, false, false);

        return Accepted(resource);
    }

    [HttpPut("monitor")]
    [Consumes("application/json")]
    public IActionResult SetEpisodesMonitored([FromBody] EpisodesMonitoredResource resource, [FromQuery] EpisodeSubresource[]? includeSubresources = null)
    {
        var includeImages = includeSubresources.Contains(EpisodeSubresource.Images);

        if (resource.RomIds.Count == 1)
        {
            _romService.SetEpisodeMonitored(resource.RomIds.First(), resource.Monitored);
        }
        else
        {
            _romService.SetMonitored(resource.RomIds, resource.Monitored);
        }

        var resources = MapToResource(_romService.GetRoms(resource.RomIds), false, false, includeImages);

        return Accepted(resources);
    }
}
