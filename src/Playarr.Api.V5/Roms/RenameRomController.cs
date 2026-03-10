using Microsoft.AspNetCore.Mvc;
using Playarr.Core.MediaFiles;
using Playarr.Http;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Roms;

[V5ApiController("rename")]
public class RenameRomController : Controller
{
    private readonly IRenameRomFileService _renameRomFileService;

    public RenameRomController(IRenameRomFileService renameRomFileService)
    {
        _renameRomFileService = renameRomFileService;
    }

    [HttpGet]
    [Produces("application/json")]
    public List<RenameRomResource> GetRoms(int gameId, int? platformNumber)
    {
        if (platformNumber.HasValue)
        {
            return _renameRomFileService.GetRenamePreviews(gameId, platformNumber.Value).ToResource();
        }

        return _renameRomFileService.GetRenamePreviews(gameId).ToResource();
    }

    [HttpGet("bulk")]
    [Produces("application/json")]
    public List<RenameRomResource> GetRoms([FromQuery] List<int> gameIds)
    {
        if (gameIds is { Count: 0 })
        {
            throw new BadRequestException("gameIds must be provided");
        }

        if (gameIds.Any(gameId => gameId <= 0))
        {
            throw new BadRequestException("gameIds must be positive integers");
        }

        return _renameRomFileService.GetRenamePreviews(gameIds).ToResource();
    }
}
