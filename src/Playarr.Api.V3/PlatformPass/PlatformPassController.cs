using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Games;
using Playarr.Http;

namespace Playarr.Api.V3.PlatformPass
{
    [V3ApiController]
    public class PlatformPassController : Controller
    {
        private readonly IGameService _seriesService;
        private readonly IEpisodeMonitoredService _episodeMonitoredService;

        public PlatformPassController(IGameService seriesService, IEpisodeMonitoredService episodeMonitoredService)
        {
            _seriesService = seriesService;
            _episodeMonitoredService = episodeMonitoredService;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult UpdateAll([FromBody] PlatformPassResource resource)
        {
            var gamesToUpdate = _seriesService.GetSeries(resource.Game.Select(s => s.Id));

            foreach (var s in resource.Game)
            {
                var game = gamesToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    game.Monitored = s.Monitored.Value;
                }

                if (s.Platforms != null && s.Platforms.Any())
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

                _episodeMonitoredService.SetEpisodeMonitoredStatus(game, resource.MonitoringOptions);
            }

            return Accepted(new object());
        }
    }
}
