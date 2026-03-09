using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using IGDB;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.DataAugmentation.DailySeries;
using Playarr.Core.Exceptions;
using Playarr.Core.Games;
using Playarr.Core.Languages;
using Playarr.Core.MediaCover;
using Playarr.Core.MetadataSource.Metacritic;

namespace Playarr.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideSeriesInfo, ISearchForNewSeries, IFetchUpcomingReleases
    {
        private const string GameFields = "fields id,name,summary,first_release_date,cover.image_id,platforms.name,platforms.abbreviation,genres.name,rating,rating_count,slug,game_status,screenshots.image_id,artworks.image_id,involved_companies.developer,involved_companies.publisher,involved_companies.company.name,dlcs,expansions;";

        private readonly IIgdbClient _igdbClient;
        private readonly Logger _logger;
        private readonly IGameService _seriesService;
        private readonly IDailyGameService _dailyGameService;
        private readonly IConfigService _configService;
        private readonly IMetacriticProxy _metacriticProxy;

        public SkyHookProxy(IIgdbClient igdbClient,
                            IGameService seriesService,
                            IDailyGameService dailyGameService,
                            IConfigService configService,
                            IMetacriticProxy metacriticProxy,
                            Logger logger)
        {
            _igdbClient = igdbClient;
            _logger = logger;
            _seriesService = seriesService;
            _dailyGameService = dailyGameService;
            _configService = configService;
            _metacriticProxy = metacriticProxy;
        }

        public Tuple<Game, List<Rom>> GetSeriesInfo(int igdbGameId)
        {
            var query = $"{GameFields} where id = {igdbGameId}; limit 1;";
            var games = _igdbClient.SearchGames(query);

            if (games == null || games.Length == 0)
            {
                throw new SeriesNotFoundException(igdbGameId);
            }

            var game = MapIgdbGame(games[0]);
            var roms = new List<Rom>();

            // Generate a base game ROM entry for each platform so the Game Details page shows content
            if (game.Platforms != null)
            {
                foreach (var platform in game.Platforms)
                {
                    roms.Add(new Rom
                    {
                        GameId = 0,
                        PlatformNumber = platform.PlatformNumber,
                        EpisodeNumber = 1,
                        Title = game.Title,
                        Overview = game.Overview,
                        AirDate = game.FirstAired?.ToString("yyyy-MM-dd"),
                        AirDateUtc = game.FirstAired?.ToUniversalTime(),
                        Ratings = game.Ratings,
                        Monitored = true
                    });
                }
            }

            // Fetch DLCs and expansions from IGDB to populate additional ROM entries per platform
            try
            {
                var dlcIds = new List<long>();

                if (games[0].Dlcs?.Ids != null)
                {
                    dlcIds.AddRange(games[0].Dlcs.Ids);
                }

                if (games[0].Expansions?.Ids != null)
                {
                    dlcIds.AddRange(games[0].Expansions.Ids);
                }

                if (dlcIds.Any())
                {
                    var idList = string.Join(",", dlcIds.Take(50));
                    var dlcQuery = $"fields id,name,first_release_date,summary,platforms.name,platforms.abbreviation; where id = ({idList}); limit 50;";
                    var dlcResults = _igdbClient.SearchGames(dlcQuery);

                    if (dlcResults != null)
                    {
                        // Sort DLCs by release date, putting items without dates at the end
                        var sortedDlcs = dlcResults
                            .OrderBy(d => d.FirstReleaseDate?.DateTime ?? DateTime.MaxValue)
                            .ToList();

                        foreach (var dlc in sortedDlcs)
                        {
                            if (game.Platforms == null)
                            {
                                continue;
                            }

                            foreach (var platform in game.Platforms)
                            {
                                var romNumber = roms.Count(r => r.PlatformNumber == platform.PlatformNumber) + 1;

                                roms.Add(new Rom
                                {
                                    GameId = 0,
                                    PlatformNumber = platform.PlatformNumber,
                                    EpisodeNumber = romNumber,
                                    Title = dlc.Name ?? "Unknown DLC",
                                    Overview = dlc.Summary,
                                    AirDate = dlc.FirstReleaseDate?.DateTime.ToString("yyyy-MM-dd"),
                                    AirDateUtc = dlc.FirstReleaseDate?.DateTime.ToUniversalTime(),
                                    Monitored = true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to fetch DLC/expansion info for game {0}", game.Title);
            }

            return new Tuple<Game, List<Rom>>(game, roms);
        }

        public List<Game> SearchForNewSeriesByImdbId(string imdbId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewSeriesByAniListId(int aniListId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewSeriesByMyAnimeListId(int malId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewSeriesByTmdbId(int tmdbId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewSeries(string title)
        {
            if (title.IsPathValid(PathValidationType.AnyOs))
            {
                throw new InvalidSearchTermException("Invalid search term '{0}'", title);
            }

            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("igdb:") || lowerTitle.StartsWith("igdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out var igdbId) || igdbId <= 0)
                    {
                        return new List<Game>();
                    }

                    try
                    {
                        var existingGame = _seriesService.FindByIgdbId(igdbId);
                        if (existingGame != null)
                        {
                            return new List<Game> { existingGame };
                        }

                        return new List<Game> { GetSeriesInfo(igdbId).Item1 };
                    }
                    catch (SeriesNotFoundException)
                    {
                        return new List<Game>();
                    }
                }

                var escapedTitle = title.Replace("\"", "\\\"").Trim();
                var query = $"{GameFields} search \"{escapedTitle}\"; limit 20;";
                var games = _igdbClient.SearchGames(query);

                if (games == null)
                {
                    return new List<Game>();
                }

                return games.Select(MapSearchResult).ToList();
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with IGDB. {1}", ex, title, ex.Message);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with IGDB. {1}", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from IGDB. {1}", ex, title, ex.Message);
            }
        }

        public List<Game> GetUpcomingReleases(DateTime start, DateTime end)
        {
            var allGames = new Dictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

            // Fetch from IGDB
            try
            {
                var igdbGames = GetIgdbUpcomingReleases(start, end);
                foreach (var game in igdbGames)
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
                _logger.Warn(ex, "Failed to fetch upcoming releases from IGDB");
            }

            // Fetch from Metacritic and merge (deduplicate by title)
            try
            {
                var metacriticGames = _metacriticProxy.GetUpcomingReleases(start, end);
                foreach (var game in metacriticGames)
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
                _logger.Warn(ex, "Failed to fetch upcoming releases from Metacritic");
            }

            return allGames.Values.ToList();
        }

        private List<Game> GetIgdbUpcomingReleases(DateTime start, DateTime end)
        {
            var startUtc = new DateTimeOffset(DateTime.SpecifyKind(start, DateTimeKind.Utc));
            var endUtc = new DateTimeOffset(DateTime.SpecifyKind(end, DateTimeKind.Utc));

            var query = $"fields game.name,game.slug,game.summary,game.cover.image_id,game.platforms.name,game.genres.name,game.rating,game.rating_count,game.game_status,date,human,platform.name,platform.abbreviation; where date >= {startUtc.ToUnixTimeSeconds()} & date <= {endUtc.ToUnixTimeSeconds()}; sort date asc; limit 50;";
            var releases = _igdbClient.SearchReleaseDates(query);

            if (releases == null)
            {
                return new List<Game>();
            }

            var games = new Dictionary<int, Game>();

            foreach (var release in releases)
            {
                var releaseGame = release.Game?.Value;
                if (releaseGame?.Id == null)
                {
                    continue;
                }

                var gameId = (int)releaseGame.Id.Value;

                if (!games.ContainsKey(gameId))
                {
                    var game = MapIgdbGame(releaseGame);
                    game.Status = GameStatusType.Upcoming;
                    game.Monitored = false;

                    if (release.Date.HasValue)
                    {
                        game.FirstAired = release.Date.Value.UtcDateTime;
                        game.Year = release.Date.Value.Year;
                    }

                    games[gameId] = game;
                }

                if (release.Platform?.Value != null)
                {
                    var platformName = release.Platform.Value.Name ?? release.Platform.Value.Abbreviation ?? $"Platform {games[gameId].Platforms.Count + 1}";
                    games[gameId].Platforms.Add(new Platform
                    {
                        PlatformNumber = games[gameId].Platforms.Count + 1,
                        Title = platformName,
                        Images = new List<MediaCover.MediaCover>(),
                        Monitored = true
                    });
                }
            }

            return games.Values.ToList();
        }

        private Game MapSearchResult(IGDB.Models.Game igdbGame)
        {
            if (igdbGame.Id == null)
            {
                return new Game();
            }

            var gameId = (int)igdbGame.Id.Value;
            var existingGame = _seriesService.FindByIgdbId(gameId);

            if (existingGame != null)
            {
                return existingGame;
            }

            return MapIgdbGame(igdbGame);
        }

        private Game MapIgdbGame(IGDB.Models.Game igdbGame)
        {
            var gameId = igdbGame.Id.HasValue ? (int)igdbGame.Id.Value : 0;
            var title = igdbGame.Name ?? string.Empty;

            var game = new Game
            {
                IgdbId = gameId,
                Title = title,
                CleanTitle = Playarr.Core.Parser.Parser.CleanGameTitle(title),
                SortTitle = GameTitleNormalizer.Normalize(title, gameId),
                Overview = igdbGame.Summary,
                TitleSlug = igdbGame.Slug,
                Status = MapIgdbStatus(igdbGame),
                OriginalLanguage = Language.English,
                Monitored = true,
                Ratings = new Ratings(),
                Images = new List<MediaCover.MediaCover>(),
                Platforms = new List<Platform>(),
                Genres = new List<string>(),
                Actors = new List<Actor>()
            };

            if (_dailyGameService.IsDailySeries(game.IgdbId))
            {
                game.SeriesType = GameTypes.Daily;
            }

            if (igdbGame.FirstReleaseDate.HasValue)
            {
                game.FirstAired = igdbGame.FirstReleaseDate.Value.UtcDateTime;
                game.Year = igdbGame.FirstReleaseDate.Value.Year;
            }

            if (igdbGame.Rating.HasValue && igdbGame.RatingCount.HasValue)
            {
                game.Ratings = new Ratings
                {
                    Value = (decimal)(igdbGame.Rating.Value / 10.0),
                    Votes = igdbGame.RatingCount.Value
                };
            }

            if (_configService.RatingSource == "metacritic")
            {
                try
                {
                    var metacriticScore = _metacriticProxy.GetMetacriticScore(game.Title, game.Year);

                    if (metacriticScore.HasValue)
                    {
                        game.Ratings = new Ratings
                        {
                            Value = metacriticScore.Value,
                            Votes = game.Ratings?.Votes ?? 0
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to fetch Metacritic score for {0}", game.Title);
                }
            }

            if (igdbGame.Genres?.Values != null)
            {
                game.Genres = igdbGame.Genres.Values
                    .Where(g => g != null && !string.IsNullOrWhiteSpace(g.Name))
                    .Select(g => g.Name)
                    .ToList();
            }

            if (igdbGame.InvolvedCompanies?.Values != null)
            {
                var developer = igdbGame.InvolvedCompanies.Values.FirstOrDefault(c => c?.Developer == true && c.Company?.Value != null);
                if (developer?.Company?.Value != null)
                {
                    game.Network = developer.Company.Value.Name;
                }
                else
                {
                    var publisher = igdbGame.InvolvedCompanies.Values.FirstOrDefault(c => c?.Publisher == true && c.Company?.Value != null);
                    if (publisher?.Company?.Value != null)
                    {
                        game.Network = publisher.Company.Value.Name;
                    }
                }
            }

            if (igdbGame.Platforms?.Values != null)
            {
                game.Platforms = igdbGame.Platforms.Values
                    .Where(p => p != null)
                    .Select((p, i) => new Platform
                    {
                        PlatformNumber = i + 1,
                        Title = p.Name ?? p.Abbreviation ?? $"Platform {i + 1}",
                        Images = new List<MediaCover.MediaCover>(),
                        Monitored = true
                    }).ToList();
            }

            var coverImageId = igdbGame.Cover?.Value?.ImageId;
            if (!string.IsNullOrWhiteSpace(coverImageId))
            {
                game.Images.Add(new MediaCover.MediaCover
                {
                    CoverType = MediaCoverTypes.Poster,
                    RemoteUrl = NormalizeImageUrl(ImageHelper.GetImageUrl(coverImageId, ImageSize.CoverBig))
                });
            }

            if (igdbGame.Screenshots?.Values != null)
            {
                foreach (var screenshot in igdbGame.Screenshots.Values.Where(s => s != null && !string.IsNullOrWhiteSpace(s.ImageId)).Take(3))
                {
                    game.Images.Add(new MediaCover.MediaCover
                    {
                        CoverType = MediaCoverTypes.Fanart,
                        RemoteUrl = NormalizeImageUrl(ImageHelper.GetImageUrl(screenshot.ImageId, ImageSize.ScreenshotBig))
                    });
                }
            }

            if (igdbGame.Artworks?.Values != null && igdbGame.Artworks.Values.Any())
            {
                foreach (var artwork in igdbGame.Artworks.Values.Where(a => a != null && !string.IsNullOrWhiteSpace(a.ImageId)).Take(2))
                {
                    game.Images.Add(new MediaCover.MediaCover
                    {
                        CoverType = MediaCoverTypes.Banner,
                        RemoteUrl = NormalizeImageUrl(ImageHelper.GetImageUrl(artwork.ImageId, ImageSize.ScreenshotBig))
                    });
                }
            }

            return game;
        }

        private static string NormalizeImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return imageUrl;
            }

            return imageUrl.StartsWith("//") ? $"https:{imageUrl}" : imageUrl;
        }

        private static GameStatusType MapIgdbStatus(IGDB.Models.Game game)
        {
            var statusId = game.GameStatus?.Id;

            // IGDB game_status IDs: 0 released, 2 alpha, 3 beta, 4 early_access, 5 offline, 6 cancelled, 7 rumored, 8 delisted
            if (!statusId.HasValue || statusId.Value == 0)
            {
                return GameStatusType.Ended;
            }

            if (statusId.Value == 2 || statusId.Value == 3 || statusId.Value == 4 || statusId.Value == 7)
            {
                return GameStatusType.Upcoming;
            }

            return GameStatusType.Continuing;
        }
    }
}
