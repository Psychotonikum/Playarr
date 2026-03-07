using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Games;
using Playarr.Http;

namespace Playarr.Api.V3.Game
{
    [V3ApiController("game/import")]
    public class GameImportController : Controller
    {
        private readonly IAddGameService _addGameService;

        public GameImportController(IAddGameService addGameService)
        {
            _addGameService = addGameService;
        }

        [HttpPost]
        public object Import([FromBody] List<GameResource> resource)
        {
            var newGame = resource.ToModel();

            return _addGameService.AddGame(newGame).ToResource();
        }
    }
}
