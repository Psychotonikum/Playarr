using Microsoft.AspNetCore.Mvc;
using Playarr.Core.MetadataSource;
using Playarr.Http;

namespace Playarr.Api.V5.Calendar
{
    [V5ApiController("calendar/igdb")]
    public class CalendarIgdbController : Controller
    {
        private readonly IFetchUpcomingReleases _upcomingReleases;

        public CalendarIgdbController(IFetchUpcomingReleases upcomingReleases)
        {
            _upcomingReleases = upcomingReleases;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<UpcomingGameResource> GetUpcomingReleases(DateTime? start, DateTime? end)
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(30);

            var games = _upcomingReleases.GetUpcomingReleases(startUse, endUse);

            return games.Select(g => new UpcomingGameResource
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
                CoverUrl = g.Images?.FirstOrDefault()?.RemoteUrl
            }).ToList();
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
    }
}
