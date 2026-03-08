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
            PlatformNumber = rom.PlatformNumber;
            EpisodeNumber = rom.EpisodeNumber;
            Title = rom.Title;
            Overview = rom.Overview;
            AirDate = rom.AirDate;
            AirDateUtc = rom.AirDateUtc;
            GameId = rom.GameId;
            IgdbId = rom.IgdbId;
            FinaleType = rom.FinaleType;
        }

        public int Id { get; set; }
        public int EpisodeNumber { get; set; }
        public int PlatformNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int GameId { get; set; }
        public int IgdbId { get; set; }
        public string FinaleType { get; set; }
    }
}
