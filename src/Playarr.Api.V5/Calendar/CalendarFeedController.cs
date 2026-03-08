using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.Tags;
using Playarr.Core.Games;
using Playarr.Http;

namespace Playarr.Api.V5.Calendar;

[V5FeedController("calendar")]
public class CalendarFeedController : Controller
{
    private readonly IRomService _episodeService;
    private readonly IGameService _seriesService;
    private readonly ITagService _tagService;

    public CalendarFeedController(IRomService episodeService, IGameService seriesService, ITagService tagService)
    {
        _episodeService = episodeService;
        _seriesService = seriesService;
        _tagService = tagService;
    }

    [HttpGet("Playarr.ics")]
    public IActionResult GetCalendarFeed(int pastDays = 7, int futureDays = 28, string tags = "", bool unmonitored = false, bool premieresOnly = false, bool asAllDay = false, bool includeSpecials = true)
    {
        var start = DateTime.Today.AddDays(-pastDays);
        var end = DateTime.Today.AddDays(futureDays);
        var parsedTags = new List<int>();

        if (tags.IsNotNullOrWhiteSpace())
        {
            parsedTags.AddRange(tags.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
        }

        var roms = _episodeService.EpisodesBetweenDates(start, end, unmonitored, includeSpecials);
        var allGames = _seriesService.GetAllSeries();
        var calendar = new Ical.Net.Calendar
        {
            ProductId = "-//playarr.tv//Playarr//EN"
        };

        var calendarName = "Playarr TV Schedule";
        calendar.AddProperty(new CalendarProperty("NAME", calendarName));
        calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

        foreach (var rom in roms.OrderBy(v => v.AirDateUtc!.Value))
        {
            var game = allGames.SingleOrDefault(s => s.Id == rom.GameId);

            if (game == null)
            {
                continue;
            }

            if (premieresOnly && (rom.PlatformNumber == 0 || rom.EpisodeNumber != 1))
            {
                continue;
            }

            if (parsedTags.Any() && parsedTags.None(game.Tags.Contains))
            {
                continue;
            }

            var occurrence = calendar.Create<CalendarEvent>();
            occurrence.Uid = "Playarr_episode_" + rom.Id;
            occurrence.Status = rom.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
            occurrence.Description = rom.Overview;
            occurrence.Categories = new List<string>() { game.Network };

            if (asAllDay)
            {
                occurrence.Start = new CalDateTime(rom.AirDateUtc!.Value.ToLocalTime()) { HasTime = false };
            }
            else
            {
                occurrence.Start = new CalDateTime(rom.AirDateUtc!.Value) { HasTime = true };
                occurrence.End = new CalDateTime(rom.AirDateUtc.Value.AddMinutes(game.Runtime)) { HasTime = true };
            }

            switch (game.SeriesType)
            {
                case GameTypes.Daily:
                    occurrence.Summary = $"{game.Title} - {rom.Title}";
                    break;
                default:
                    occurrence.Summary = $"{game.Title} - {rom.PlatformNumber}x{rom.EpisodeNumber:00} - {rom.Title}";
                    break;
            }
        }

        var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
        var icalendar = serializer.SerializeToString(calendar);

        return Content(icalendar, "text/calendar");
    }
}
