using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;
using Playarr.Http.REST.Attributes;

namespace Playarr.Api.V3.Roms
{
    [V3ApiController]
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
        public List<RomResource> GetEpisodes(int? gameId, int? platformNumber, [FromQuery]List<int> romIds, int? romFileId, bool includeSeries = false, bool includeRomFile = false, bool includeImages = false)
        {
            if (gameId.HasValue)
            {
                if (platformNumber.HasValue)
                {
                    return MapToResource(_episodeService.GetEpisodesBySeason(gameId.Value, platformNumber.Value), includeSeries, includeRomFile, includeImages);
                }

                return MapToResource(_episodeService.GetEpisodeBySeries(gameId.Value), includeSeries, includeRomFile, includeImages);
            }
            else if (romIds.Any())
            {
                return MapToResource(_episodeService.GetEpisodes(romIds), includeSeries, includeRomFile, includeImages);
            }
            else if (romFileId.HasValue)
            {
                return MapToResource(_episodeService.GetEpisodesByFileId(romFileId.Value), includeSeries, includeRomFile, includeImages);
            }

            throw new BadRequestException("gameId or romIds must be provided");
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<RomResource> SetEpisodeMonitored([FromRoute] int id, [FromBody] RomResource resource)
        {
            _episodeService.SetEpisodeMonitored(id, resource.Monitored);

            resource = MapToResource(_episodeService.GetEpisode(id), false, false, false);

            return Accepted(resource);
        }

        [HttpPut("monitor")]
        [Consumes("application/json")]
        public IActionResult SetEpisodesMonitored([FromBody] EpisodesMonitoredResource resource, [FromQuery] bool includeImages = false)
        {
            if (resource.RomIds.Count == 1)
            {
                _episodeService.SetEpisodeMonitored(resource.RomIds.First(), resource.Monitored);
            }
            else
            {
                _episodeService.SetMonitored(resource.RomIds, resource.Monitored);
            }

            var resources = MapToResource(_episodeService.GetEpisodes(resource.RomIds), false, false, includeImages);

            return Accepted(resources);
        }
    }
}
