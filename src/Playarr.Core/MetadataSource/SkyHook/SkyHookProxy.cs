using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using IGDB;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.DataAugmentation.DailySeries;
using Playarr.Core.Exceptions;
using Playarr.Core.Games;
using Playarr.Core.Languages;
using Playarr.Core.MediaCover;

namespace Playarr.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideSeriesInfo, ISearchForNewSeries, IFetchUpcomingReleases
    {
        private const string GameFields = "fields id,name,summary,first_release_date,cover.image_id,platforms.name,platforms.abbreviation,genres.name,rating,rating_count,slug,game_status,screenshots.image_id,artworks.image_id,involved_companies.developer,involved_companies.publisher,involved_companies.company.name;";

        private readonly IIgdbClient _igdbClient;
        private readonly Logger _logger;
        private readonly IGameService _seriesService;
        private readonly IDailyGameService _dailyGameService;

        public SkyHookProxy(IIgdbClient igdbClient,
                            IGameService seriesService,
                            IDailyGameService dailyGameService,
                            Logger logger)
        {
            _igdbClient = igdbClient;
            _logger = logger;
            _seriesService = seriesService;
            _dailyGameService = dailyGameService;
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
            try
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
                        games[gameId].Platforms.Add(new Platform
                        {
                            PlatformNumber = games[gameId].Platforms.Count + 1,
                            Images = new List<MediaCover.MediaCover>(),
                            Monitored = true
                        });
                    }
                }

                return games.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch upcoming releases from IGDB");
                return new List<Game>();
            }
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
                game.Platforms = igdbGame.Platforms.Values.Select((_, i) => new Platform
                {
                    PlatformNumber = i + 1,
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
