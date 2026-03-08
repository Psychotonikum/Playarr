using System.Text.Json.Serialization;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata.Consumers.Xbmc
{
    public class KodiEpisodeGuide
    {
        [JsonPropertyName("igdb")]
        public string Igdb { get; set; }

        [JsonPropertyName("tvmaze")]
        public string TvMaze { get; set; }

        [JsonPropertyName("tvrage")]
        public string TvRage { get; set; }

        [JsonPropertyName("tmdb")]
        public string Tmdb { get; set; }

        [JsonPropertyName("imdb")]
        public string Imdb { get; set; }

        public KodiEpisodeGuide()
        {
        }

        public KodiEpisodeGuide(Game game)
        {
            Igdb = game.IgdbId.ToString();
            TvMaze = game.RawgId > 0 ? game.RawgId.ToString() : null;
            TvRage = game.MobyGamesId > 0 ? game.RawgId.ToString() : null;
            Tmdb = game.TmdbId > 0 ? game.TmdbId.ToString() : null;
            Imdb = game.ImdbId;
        }
    }
}
