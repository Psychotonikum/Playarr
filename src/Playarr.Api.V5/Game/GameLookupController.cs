using Microsoft.AspNetCore.Mvc;
using Playarr.Core.ImportLists.Exclusions;
using Playarr.Core.MediaCover;
using Playarr.Core.MetadataSource;
using Playarr.Core.Organizer;
using Playarr.Core.GameStats;
using Playarr.Http;

namespace Playarr.Api.V5.Game;

[V5ApiController("game/lookup")]
public class GameLookupController : Controller
{
    private readonly ISearchForNewSeries _searchProxy;
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly IMapCoversToLocal _coverMapper;
    private readonly IImportListExclusionService _importListExclusionService;

    public GameLookupController(ISearchForNewSeries searchProxy, IBuildFileNames fileNameBuilder, IMapCoversToLocal coverMapper,  IImportListExclusionService importListExclusionService)
    {
        _searchProxy = searchProxy;
        _fileNameBuilder = fileNameBuilder;
        _coverMapper = coverMapper;
        _importListExclusionService = importListExclusionService;
    }

    [HttpGet]
    public IEnumerable<GameResource> Search([FromQuery] string term)
    {
        var tvDbResults = _searchProxy.SearchForNewSeries(term);
        return MapToResource(tvDbResults);
    }

    private IEnumerable<GameResource> MapToResource(IEnumerable<Playarr.Core.Games.Game> game)
    {
        foreach (var currentSeries in game)
        {
            var resource = currentSeries.ToResource();

            _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

            var poster = currentSeries.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);

            if (poster != null)
            {
                resource.RemotePoster = poster.RemoteUrl;
            }

            resource.Folder = _fileNameBuilder.GetGameFolder(currentSeries);
            resource.Statistics = new SeriesStatistics().ToResource(resource.Platforms);
            resource.IsExcluded = _importListExclusionService.FindByIgdbId(currentSeries.TvdbId) is not null;

            yield return resource;
        }
    }
}
