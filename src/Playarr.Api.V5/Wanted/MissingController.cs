using Microsoft.AspNetCore.Mvc;
using Playarr.Core.CustomFormats;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Api.V5.Roms;
using Playarr.Http;
using Playarr.Http.Extensions;

namespace Playarr.Api.V5.Wanted;

[V5ApiController("wanted/missing")]
public class MissingController : RomControllerWithSignalR
{
    public MissingController(IRomService episodeService,
                         IGameService seriesService,
                         IUpgradableSpecification upgradableSpecification,
                         ICustomFormatCalculationService formatCalculator,
                         IBroadcastSignalRMessage signalRBroadcaster)
        : base(episodeService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
    {
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<RomResource> GetMissingEpisodes([FromQuery] PagingRequestResource paging, bool monitored = true, [FromQuery] MissingSubresource[]? includeSubresources = null)
    {
        var pagingResource = new PagingResource<RomResource>(paging);
        var pagingSpec = pagingResource.MapToPagingSpec<RomResource, Rom>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "roms.airDateUtc",
                "roms.lastSearchTime",
                "game.sortTitle"
            },
            "roms.airDateUtc",
            SortDirection.Ascending);

        if (monitored)
        {
            pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Game.Monitored == true);
        }
        else
        {
            pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Game.Monitored == false);
        }

        var includeSeries = includeSubresources.Contains(MissingSubresource.Game);
        var includeImages = includeSubresources.Contains(MissingSubresource.Images);

        var resource = pagingSpec.ApplyToPage(_episodeService.EpisodesWithoutFiles, v => MapToResource(v, includeSeries, false, includeImages));

        return resource;
    }
}
