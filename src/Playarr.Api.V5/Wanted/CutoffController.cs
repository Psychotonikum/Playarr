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

[V5ApiController("wanted/cutoff")]
public class CutoffController : RomControllerWithSignalR
{
    private readonly IEpisodeCutoffService _episodeCutoffService;

    public CutoffController(IEpisodeCutoffService episodeCutoffService,
                        IRomService episodeService,
                        IGameService seriesService,
                        IUpgradableSpecification upgradableSpecification,
                        ICustomFormatCalculationService formatCalculator,
                        IBroadcastSignalRMessage signalRBroadcaster)
        : base(episodeService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
    {
        _episodeCutoffService = episodeCutoffService;
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<RomResource> GetCutoffUnmetEpisodes([FromQuery] PagingRequestResource paging, bool monitored = true, [FromQuery] CutoffSubresource[]? includeSubresources = null)
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

        var includeSeries = includeSubresources.Contains(CutoffSubresource.Game);
        var includeRomFile = includeSubresources.Contains(CutoffSubresource.RomFile);
        var includeImages = includeSubresources.Contains(CutoffSubresource.Images);

        var resource = pagingSpec.ApplyToPage(_episodeCutoffService.EpisodesWhereCutoffUnmet, v => MapToResource(v, includeSeries, includeRomFile, includeImages));

        return resource;
    }
}
