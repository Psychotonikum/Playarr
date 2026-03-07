using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Languages;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookSeries
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int TvdbId { get; set; }
        public int RawgId { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public HashSet<int> MalIds { get; set; }
        public HashSet<int> AniListIds { get; set; }
        public GameTypes Type { get; set; }
        public int Year { get; set; }
        public List<string> Genres { get; set; }
        public List<WebhookImage> Images { get; set; }
        public List<string> Tags { get; set; }
        public Language OriginalLanguage { get; set; }
        public string OriginalCountry { get; set; }

        public WebhookSeries()
        {
        }

        public WebhookSeries(Game game, List<string> tags)
        {
            Id = game.Id;
            Title = game.Title;
            TitleSlug = game.TitleSlug;
            Path = game.Path;
            TvdbId = game.TvdbId;
            RawgId = game.RawgId;
            TmdbId = game.TmdbId;
            ImdbId = game.ImdbId;
            MalIds = game.MalIds;
            AniListIds = game.AniListIds;
            Type = game.SeriesType;
            Year = game.Year;
            Genres = game.Genres;
            Images = game.Images.Select(i => new WebhookImage(i)).ToList();
            Tags = tags;
            OriginalLanguage = game.OriginalLanguage;
            OriginalCountry = game.OriginalCountry;
        }
    }
}
