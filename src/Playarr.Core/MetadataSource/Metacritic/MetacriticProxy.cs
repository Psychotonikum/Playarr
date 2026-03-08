using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using NLog;
using Playarr.Common.Http;
using Playarr.Core.Games;

namespace Playarr.Core.MetadataSource.Metacritic
{
    public interface IMetacriticProxy
    {
        List<Game> GetUpcomingReleases(DateTime start, DateTime end);
        decimal? GetMetacriticScore(string gameTitle, int? year);
    }

    public class MetacriticProxy : IMetacriticProxy
    {
        private const string MetacriticApiBase = "https://internal-prod.apigee.fandom.net/v2";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MetacriticProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<Game> GetUpcomingReleases(DateTime start, DateTime end)
        {
            try
            {
                var request = new HttpRequest($"{MetacriticApiBase}/games/upcoming")
                {
                    AllowAutoRedirect = true,
                    RequestTimeout = TimeSpan.FromSeconds(15)
                };

                request.Headers.Add("User-Agent", "Playarr/1.0");

                var response = _httpClient.Get(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Warn("Metacritic API returned status {0}", response.StatusCode);
                    return new List<Game>();
                }

                var games = ParseUpcomingGames(response.Content, start, end);
                return games;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch upcoming releases from Metacritic");
                return new List<Game>();
            }
        }

        public decimal? GetMetacriticScore(string gameTitle, int? year)
        {
            try
            {
                var searchTitle = Uri.EscapeDataString(gameTitle);
                var request = new HttpRequest($"{MetacriticApiBase}/search/game/{searchTitle}")
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

                using var doc = JsonDocument.Parse(response.Content);
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

                    // Fallback: return first result's score
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
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to get Metacritic score for {0}", gameTitle);
                return null;
            }
        }

        private List<Game> ParseUpcomingGames(string content, DateTime start, DateTime end)
        {
            var games = new List<Game>();

            try
            {
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
                        _logger.Debug(ex, "Failed to parse Metacritic game entry");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to parse Metacritic upcoming games response");
            }

            return games;
        }
    }
}
