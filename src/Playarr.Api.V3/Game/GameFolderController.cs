using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Organizer;
using Playarr.Core.Games;
using Playarr.Http;

namespace Playarr.Api.V3.Game;

[V3ApiController("game")]
public class GameFolderController : Controller
{
    private readonly IGameService _gameService;
    private readonly IBuildFileNames _fileNameBuilder;

    public GameFolderController(IGameService seriesService, IBuildFileNames fileNameBuilder)
    {
        _gameService = seriesService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id}/folder")]
    public object GetFolder([FromRoute] int id)
    {
        var game = _gameService.GetGame(id);
        var folder = _fileNameBuilder.GetGameFolder(game);

        return new
        {
            folder
        };
    }
}
