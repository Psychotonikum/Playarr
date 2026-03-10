using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Games;
using Playarr.Http;

namespace Playarr.Api.V5.PlatformPass;

[V5ApiController]
public class PlatformPassController : Controller
{
    private readonly IGameService _gameService;
    private readonly IEpisodeMonitoredService _romMonitoredService;

    public PlatformPassController(IGameService seriesService, IEpisodeMonitoredService episodeMonitoredService)
    {
        _gameService = seriesService;
        _romMonitoredService = episodeMonitoredService;
    }

    [HttpPost]
    [Consumes("application/json")]
    public IActionResult UpdateAll([FromBody] PlatformPassResource resource)
    {
        var gamesToUpdate = _gameService.GetGame(resource.Game.Select(s => s.Id));

        foreach (var s in resource.Game)
        {
            var game = gamesToUpdate.Single(c => c.Id == s.Id);

            if (s.Monitored.HasValue)
            {
                game.Monitored = s.Monitored.Value;
            }

            if (s.Platforms.Any())
            {
                foreach (var seriesSeason in game.Platforms)
                {
                    var platform = s.Platforms.FirstOrDefault(c => c.PlatformNumber == seriesSeason.PlatformNumber);

                    if (platform != null)
                    {
                        seriesSeason.Monitored = platform.Monitored;
                    }
                }
            }

            if (resource.MonitoringOptions != null && resource.MonitoringOptions.Monitor == MonitorTypes.None)
            {
                game.Monitored = false;
            }

            _romMonitoredService.SetEpisodeMonitoredStatus(game, resource.MonitoringOptions.ToModel());
        }

        return NoContent();
    }
}
