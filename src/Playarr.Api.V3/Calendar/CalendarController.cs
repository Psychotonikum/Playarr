using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Tags;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Api.V3.Roms;
using Playarr.Http;

namespace Playarr.Api.V3.Calendar
{
    [V3ApiController]
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
        public List<RomResource> GetCalendar(DateTime? start, DateTime? end, bool unmonitored = false, bool includeSeries = false, bool includeRomFile = false, bool includeEpisodeImages = false, string tags = "")
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(2);
            var roms = _episodeService.EpisodesBetweenDates(startUse, endUse, unmonitored, true);
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

            var resources = MapToResource(result, includeSeries, includeRomFile, includeEpisodeImages);

            return resources.OrderBy(e => e.AirDateUtc).ToList();
        }
    }
}
