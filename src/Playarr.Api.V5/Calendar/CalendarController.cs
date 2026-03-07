using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Tags;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Api.V5.Roms;
using Playarr.Http;

namespace Playarr.Api.V5.Calendar
{
    [V5ApiController]
    public class CalendarController : RomControllerWithSignalR
    {
        private readonly ITagService _tagService;

        public CalendarController(IBroadcastSignalRMessage signalR,
                            IRomService episodeService,
                            IGameService seriesService,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            ITagService tagService,
                            ICustomFormatCalculationService formatCalculator)
            : base(episodeService, seriesService, qualityUpgradableSpecification, formatCalculator, signalR)
        {
            _tagService = tagService;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<RomResource> GetCalendar(DateTime? start, DateTime? end, bool includeUnmonitored = false, bool includeSpecials = true, string tags = "", [FromQuery] CalendarSubresource[]? includeSubresources = null)
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(2);
            var roms = _episodeService.EpisodesBetweenDates(startUse, endUse, includeUnmonitored, includeSpecials);
            var allGames = _seriesService.GetAllSeries();
            var parsedTags = new List<int>();
            var result = new List<Rom>();

            if (tags.IsNotNullOrWhiteSpace())
            {
                parsedTags.AddRange(tags.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            foreach (var rom in roms)
            {
                var game = allGames.SingleOrDefault(s => s.Id == rom.SeriesId);

                if (game == null)
                {
                    continue;
                }

                if (parsedTags.Any() && parsedTags.None(game.Tags.Contains))
                {
                    continue;
                }

                result.Add(rom);
            }

            var includeSeries = includeSubresources.Contains(CalendarSubresource.Game);
            var includeRomFile = includeSubresources.Contains(CalendarSubresource.RomFile);
            var includeEpisodeImages = includeSubresources.Contains(CalendarSubresource.Images);

            var resources = MapToResource(result, includeSeries, includeRomFile, includeEpisodeImages);

            return resources.OrderBy(e => e.AirDateUtc).ToList();
        }
    }
}
