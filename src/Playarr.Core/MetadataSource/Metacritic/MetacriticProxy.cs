using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;
using Playarr.Common.Http;
using Playarr.Core.Games;
using Playarr.Core.MediaCover;

namespace Playarr.Core.MetadataSource.Metacritic
{
    public interface IMetacriticProxy
    {
        List<Game> GetUpcomingReleases(DateTime start, DateTime end);
        decimal? GetMetacriticScore(string gameTitle, int? year);
    }

    /// <summary>
    /// Metacritic data provider using the chrismichaelps/metacritic URL patterns for HTML
    /// scraping (primary) with the Fandom API as fallback. Ported from the unofficial-metacritic
    /// npm package (https://github.com/chrismichaelps/metacritic).
    /// </summary>
    public class MetacriticProxy : IMetacriticProxy
    {
        // chrismichaelps/metacritic URL patterns
        private const string MetacriticBase = "https://www.metacritic.com";
        private const string BrowseGamesUrl = MetacriticBase + "/browse/games/release-date/{0}/{1}/{2}";

        // Fandom API fallback
        private const string FandomApiBase = "https://internal-prod.apigee.fandom.net/v2";

        private static readonly string[] Platforms = { "pc", "ps5", "ps4", "xbox-series-x", "xboxone", "switch" };

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MetacriticProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<Game> GetUpcomingReleases(DateTime start, DateTime end)
        {
            var games = new List<Game>();

            // Primary: scrape Metacritic browse pages (chrismichaelps approach)
            try
            {
                games = ScrapeComingSoonGames(start, end);

                if (games.Count > 0)
                {
                    _logger.Debug("Found {0} upcoming games from Metacritic scraping", games.Count);
                    return games;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Metacritic HTML scraping failed, falling back to Fandom API");
            }

            // Fallback: Fandom API
            try
            {
                var request = new HttpRequest($"{FandomApiBase}/games/upcoming")
                {
                    AllowAutoRedirect = true,
                    RequestTimeout = TimeSpan.FromSeconds(15)
                };

                request.Headers.Add("User-Agent", "Playarr/1.0");

                var response = _httpClient.Get(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    games = ParseFandomUpcomingGames(response.Content, start, end);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch upcoming releases from Metacritic (all sources)");
            }

            return games;
        }

        public decimal? GetMetacriticScore(string gameTitle, int? year)
        {
            // Primary: scrape the game's Metacritic page for JSON-LD aggregateRating
            try
            {
                var score = ScrapeGameScore(gameTitle);

                if (score.HasValue)
                {
                    return score;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Metacritic HTML score scraping failed for {0}", gameTitle);
            }

            // Fallback: Fandom API search
            try
            {
                var searchTitle = Uri.EscapeDataString(gameTitle);
                var request = new HttpRequest($"{FandomApiBase}/search/game/{searchTitle}")
                {
                    AllowAutoRedirect = true,
                    RequestTimeout = TimeSpan.FromSeconds(10)
                };

                request.Headers.Add("User-Agent", "Playarr/1.0");

                var response = _httpClient.Get(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                return ParseFandomSearchScore(response.Content, gameTitle);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to get Metacritic score for {0}", gameTitle);
                return null;
            }
        }

        // --- chrismichaelps-style HTML scraping (ported from unofficial-metacritic) ---

        private List<Game> ScrapeComingSoonGames(DateTime start, DateTime end)
        {
            var allGames = new Dictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

            // Scrape coming-soon for each platform (mirrors chrismichaelps getGameReviews)
            foreach (var platform in Platforms)
            {
                try
                {
                    var url = string.Format(BrowseGamesUrl, "coming-soon", platform, "date");
                    var html = FetchPage(url);

                    if (string.IsNullOrWhiteSpace(html))
                    {
                        continue;
                    }

                    var scraped = ParseBrowseGamesHtml(html, platform, start, end);

                    foreach (var game in scraped)
                    {
                        var key = game.CleanTitle ?? game.Title?.ToLowerInvariant() ?? "";

                        if (!allGames.ContainsKey(key))
                        {
                            allGames[key] = game;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to scrape Metacritic coming-soon for platform {0}", platform);
                }
            }

            return allGames.Values.ToList();
        }

        private decimal? ScrapeGameScore(string gameTitle)
        {
            // Convert game title to Metacritic slug format
            var slug = Regex.Replace(gameTitle.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

            // Try common platforms
            foreach (var platform in Platforms)
            {
                try
                {
                    var url = $"{MetacriticBase}/game/{slug}/{platform}";
                    var html = FetchPage(url);

                    if (string.IsNullOrWhiteSpace(html))
                    {
                        continue;
                    }

                    // Extract JSON-LD structured data (chrismichaelps approach: script type="application/ld+json")
                    var jsonLdMatch = Regex.Match(html, @"<script[^>]*type=""application/ld\+json""[^>]*>(.*?)</script>", RegexOptions.Singleline);

                    if (jsonLdMatch.Success)
                    {
                        using var doc = JsonDocument.Parse(jsonLdMatch.Groups[1].Value);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("aggregateRating", out var rating) &&
                            rating.TryGetProperty("ratingValue", out var ratingValue))
                        {
                            var rawScore = ratingValue.ValueKind == JsonValueKind.Number
                                ? ratingValue.GetDecimal()
                                : decimal.TryParse(ratingValue.GetString(), out var parsed) ? parsed : (decimal?)null;

                            if (rawScore.HasValue)
                            {
                                // Metacritic scores are 0-100, normalize to 0-10
                                return rawScore.Value / 10m;
                            }
                        }
                    }

                    // Fallback: extract metascore from HTML (chrismichaelps css selector approach)
                    var scoreMatch = Regex.Match(html, @"class=""metascore_w[^""]*""\s*>(\d+)<");

                    if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var metascore))
                    {
                        return metascore / 10m;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to scrape Metacritic score from {0}/{1}", platform, slug);
                }
            }

            return null;
        }

        private List<Game> ParseBrowseGamesHtml(string html, string platformSlug, DateTime start, DateTime end)
        {
            var games = new List<Game>();

            // chrismichaelps parses: td.clamp-summary-wrap a.title h3 (title),
            //   div.metascore_w (score), div.clamp-details span (platform + release_date),
            //   div.summary (summary), td.clamp-image-wrap a img (poster)
            var rowPattern = new Regex(
                @"<tr\b[^>]*>(.*?)</tr>",
                RegexOptions.Singleline);

            var titlePattern = new Regex(
                @"class=""title""[^>]*>.*?<h3>(.*?)</h3>",
                RegexOptions.Singleline);

            var scorePattern = new Regex(
                @"class=""metascore_w[^""]*""\s*>(\d+)<",
                RegexOptions.Singleline);

            var detailsPattern = new Regex(
                @"class=""clamp-details""[^>]*>(.*?)</div>",
                RegexOptions.Singleline);

            var summaryPattern = new Regex(
                @"class=""summary""[^>]*>(.*?)</div>",
                RegexOptions.Singleline);

            var posterPattern = new Regex(
                @"class=""clamp-image-wrap"".*?<img[^>]*src=""([^""]+)""",
                RegexOptions.Singleline);

            var platformName = MapPlatformSlug(platformSlug);

            foreach (Match row in rowPattern.Matches(html))
            {
                try
                {
                    var rowHtml = row.Groups[1].Value;

                    var titleMatch = titlePattern.Match(rowHtml);

                    if (!titleMatch.Success)
                    {
                        continue;
                    }

                    var title = WebUtility.HtmlDecode(titleMatch.Groups[1].Value).Trim();

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        continue;
                    }

                    DateTime? releaseDate = null;
                    var detailsMatch = detailsPattern.Match(rowHtml);

                    if (detailsMatch.Success)
                    {
                        // Extract date from spans inside clamp-details
                        var spans = Regex.Matches(detailsMatch.Groups[1].Value, @"<span[^>]*>(.*?)</span>", RegexOptions.Singleline);
                        foreach (Match span in spans)
                        {
                            var spanText = WebUtility.HtmlDecode(span.Groups[1].Value).Trim();

                            if (DateTime.TryParse(spanText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                            {
                                releaseDate = parsed;
                                break;
                            }
                        }
                    }

                    if (releaseDate.HasValue && (releaseDate.Value < start || releaseDate.Value > end))
                    {
                        continue;
                    }

                    var game = new Game
                    {
                        Title = title,
                        CleanTitle = Playarr.Core.Parser.Parser.CleanGameTitle(title),
                        Status = GameStatusType.Upcoming,
                        Monitored = false,
                        FirstAired = releaseDate,
                        Year = releaseDate?.Year ?? 0,
                        Images = new List<MediaCover.MediaCover>(),
                        Platforms = new List<Platform>
                        {
                            new Platform
                            {
                                PlatformNumber = 1,
                                Title = platformName,
                                Images = new List<MediaCover.MediaCover>(),
                                Monitored = true
                            }
                        },
                        Genres = new List<string>(),
                        Actors = new List<Actor>(),
                        Ratings = new Ratings()
                    };

                    var scoreMatch2 = scorePattern.Match(rowHtml);

                    if (scoreMatch2.Success && int.TryParse(scoreMatch2.Groups[1].Value, out var score))
                    {
                        game.Ratings = new Ratings { Value = score / 10m, Votes = 0 };
                    }

                    var summaryMatch = summaryPattern.Match(rowHtml);

                    if (summaryMatch.Success)
                    {
                        game.Overview = WebUtility.HtmlDecode(summaryMatch.Groups[1].Value).Trim();
                    }

                    var posterMatch = posterPattern.Match(rowHtml);

                    if (posterMatch.Success)
                    {
                        var posterUrl = posterMatch.Groups[1].Value;

                        if (!string.IsNullOrWhiteSpace(posterUrl) && posterUrl != "null")
                        {
                            game.Images.Add(new MediaCover.MediaCover
                            {
                                CoverType = MediaCoverTypes.Poster,
                                RemoteUrl = posterUrl.StartsWith("//") ? $"https:{posterUrl}" : posterUrl
                            });
                        }
                    }

                    games.Add(game);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to parse Metacritic browse row");
                }
            }

            return games;
        }

        private string FetchPage(string url)
        {
            var request = new HttpRequest(url)
            {
                AllowAutoRedirect = true,
                RequestTimeout = TimeSpan.FromSeconds(15)
            };

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            var response = _httpClient.Get(request);

            return response.StatusCode == HttpStatusCode.OK ? response.Content : null;
        }

        private static string MapPlatformSlug(string slug)
        {
            return slug switch
            {
                "ps5" => "PlayStation 5",
                "ps4" => "PlayStation 4",
                "xbox-series-x" => "Xbox Series X",
                "xboxone" => "Xbox One",
                "switch" => "Nintendo Switch",
                "pc" => "PC",
                "ios" => "iOS",
                "stadia" => "Stadia",
                _ => slug
            };
        }

        // --- Fandom API fallback ---

        private decimal? ParseFandomSearchScore(string content, string gameTitle)
        {
            using var doc = JsonDocument.Parse(content);
            var results = doc.RootElement;

            if (results.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    if (item.TryGetProperty("title", out var title) &&
                        title.GetString()?.Equals(gameTitle, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (item.TryGetProperty("score", out var score) && score.ValueKind == JsonValueKind.Number)
                        {
                            return score.GetDecimal();
                        }
                    }
                }

                if (data.GetArrayLength() > 0)
                {
                    var first = data[0];

                    if (first.TryGetProperty("score", out var firstScore) && firstScore.ValueKind == JsonValueKind.Number)
                    {
                        return firstScore.GetDecimal();
                    }
                }
            }

            return null;
        }

        private List<Game> ParseFandomUpcomingGames(string content, DateTime start, DateTime end)
        {
            var games = new List<Game>();

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            JsonElement items;

            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                items = data;
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                items = root;
            }
            else
            {
                return games;
            }

            foreach (var item in items.EnumerateArray())
            {
                try
                {
                    var title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        continue;
                    }

                    DateTime? releaseDate = null;

                    if (item.TryGetProperty("releaseDate", out var relDate) && relDate.ValueKind == JsonValueKind.String)
                    {
                        if (DateTime.TryParse(relDate.GetString(), out var parsed))
                        {
                            releaseDate = parsed;
                        }
                    }

                    if (releaseDate.HasValue && (releaseDate.Value < start || releaseDate.Value > end))
                    {
                        continue;
                    }

                    var game = new Game
                    {
                        Title = title,
                        CleanTitle = Playarr.Core.Parser.Parser.CleanGameTitle(title),
                        Status = GameStatusType.Upcoming,
                        Monitored = false,
                        FirstAired = releaseDate,
                        Year = releaseDate?.Year ?? 0,
                        Images = new List<MediaCover.MediaCover>(),
                        Platforms = new List<Platform>(),
                        Genres = new List<string>(),
                        Actors = new List<Actor>(),
                        Ratings = new Ratings()
                    };

                    if (item.TryGetProperty("description", out var desc))
                    {
                        game.Overview = desc.GetString();
                    }

                    if (item.TryGetProperty("platforms", out var platformsProp) && platformsProp.ValueKind == JsonValueKind.Array)
                    {
                        var idx = 1;

                        foreach (var plat in platformsProp.EnumerateArray())
                        {
                            game.Platforms.Add(new Platform
                            {
                                PlatformNumber = idx++,
                                Title = plat.GetString() ?? $"Platform {idx}",
                                Images = new List<MediaCover.MediaCover>(),
                                Monitored = true
                            });
                        }
                    }

                    games.Add(game);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to parse Fandom API game entry");
                }
            }

            return games;
        }
    }
}
