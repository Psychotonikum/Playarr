using Microsoft.AspNetCore.Mvc;
using Playarr.Core.MetadataSource;
using Playarr.Core.MetadataSource.Metacritic;
using Playarr.Http;

namespace Playarr.Api.V5.Calendar
{
    [V5ApiController("calendar/igdb")]
    public class CalendarIgdbController : Controller
    {
        private readonly IFetchUpcomingReleases _upcomingReleases;
        private readonly IMetacriticProxy _metacriticProxy;

        public CalendarIgdbController(IFetchUpcomingReleases upcomingReleases, IMetacriticProxy metacriticProxy)
        {
            _upcomingReleases = upcomingReleases;
            _metacriticProxy = metacriticProxy;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<UpcomingGameResource> GetUpcomingReleases(DateTime? start, DateTime? end)
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(30);

            var igdbGames = _upcomingReleases.GetUpcomingReleases(startUse, endUse);
            var metacriticGames = _metacriticProxy.GetUpcomingReleases(startUse, endUse);

            var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<UpcomingGameResource>();

            foreach (var g in igdbGames)
            {
                var title = g.Title ?? string.Empty;
                seenTitles.Add(title);
                results.Add(MapToResource(g, "igdb"));
            }

            foreach (var g in metacriticGames)
            {
                var title = g.Title ?? string.Empty;
                if (!seenTitles.Contains(title))
                {
                    seenTitles.Add(title);
                    results.Add(MapToResource(g, "metacritic"));
                }
            }

            return results.OrderBy(r => r.ReleaseDate).ToList();
        }

        private static UpcomingGameResource MapToResource(Playarr.Core.Games.Game g, string source)
        {
            return new UpcomingGameResource
            {
                IgdbId = g.IgdbId,
                Title = g.Title ?? string.Empty,
                Overview = g.Overview ?? string.Empty,
                ReleaseDate = g.FirstAired,
                Year = g.Year,
                Network = g.Network ?? string.Empty,
                Status = g.Status.ToString(),
                Genres = g.Genres ?? [],
                PlatformCount = g.Platforms?.Count ?? 0,
                CoverUrl = g.Images?.FirstOrDefault()?.RemoteUrl,
                Source = source
            };
        }
    }

    public class UpcomingGameResource
    {
        public int IgdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int Year { get; set; }
        public string? Network { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = [];
        public int PlatformCount { get; set; }
        public string? CoverUrl { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
