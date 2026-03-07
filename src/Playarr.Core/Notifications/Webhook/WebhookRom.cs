using System;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookEpisode
    {
        public WebhookEpisode()
        {
        }

        public WebhookEpisode(Rom rom)
        {
            Id = rom.Id;
            SeasonNumber = rom.SeasonNumber;
            EpisodeNumber = rom.EpisodeNumber;
            Title = rom.Title;
            Overview = rom.Overview;
            AirDate = rom.AirDate;
            AirDateUtc = rom.AirDateUtc;
            SeriesId = rom.SeriesId;
            TvdbId = rom.TvdbId;
            FinaleType = rom.FinaleType;
        }

        public int Id { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int SeriesId { get; set; }
        public int TvdbId { get; set; }
        public string FinaleType { get; set; }
    }
}
