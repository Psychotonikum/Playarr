using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using NLog;
using Playarr.Common.Cloud;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.DataAugmentation.DailySeries;
using Playarr.Core.Exceptions;
using Playarr.Core.Languages;
using Playarr.Core.MediaCover;
using Playarr.Core.MetadataSource.SkyHook.Resource;
using Playarr.Core.Parser;
using Playarr.Core.Games;

namespace Playarr.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideSeriesInfo, ISearchForNewSeries
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly IGameService _seriesService;
        private readonly IDailyGameService _dailyGameService;
        private readonly IHttpRequestBuilderFactory _requestBuilder;

        public SkyHookProxy(IHttpClient httpClient,
                            IPlayarrCloudRequestBuilder requestBuilder,
                            IGameService seriesService,
                            IDailyGameService dailyGameService,
                            Logger logger)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder.SkyHookTvdb;
            _logger = logger;
            _seriesService = seriesService;
            _dailyGameService = dailyGameService;
            _requestBuilder = requestBuilder.SkyHookTvdb;
        }

        public Tuple<Game, List<Rom>> GetSeriesInfo(int tvdbGameId)
        {
            var httpRequest = _requestBuilder.Create()
                                             .SetSegment("route", "shows")
                                             .Resource(tvdbGameId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<ShowResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SeriesNotFoundException(tvdbGameId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var roms = httpResponse.Resource.Roms.Select(MapEpisode);
            var game = MapSeries(httpResponse.Resource);

            return new Tuple<Game, List<Rom>>(game, roms.ToList());
        }

        public List<Game> SearchForNewSeriesByImdbId(string imdbId)
        {
            imdbId = Parser.Parser.NormalizeImdbId(imdbId);

            if (imdbId == null)
            {
                return new List<Game>();
            }

            var results = SearchForNewSeries($"imdb:{imdbId}");

            return results;
        }

        public List<Game> SearchForNewSeriesByAniListId(int aniListId)
        {
            var results = SearchForNewSeries($"anilist:{aniListId}");

            return results;
        }

        public List<Game> SearchForNewSeriesByMyAnimeListId(int malId)
        {
            var results = SearchForNewSeries($"mal:{malId}");

            return results;
        }

        public List<Game> SearchForNewSeriesByTmdbId(int tmdbId)
        {
            var results = SearchForNewSeries($"tmdb:{tmdbId}");

            return results;
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

                if (lowerTitle.StartsWith("tvdb:") || lowerTitle.StartsWith("tvdbid:"))
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

                var httpRequest = _requestBuilder.Create()
                                                 .SetSegment("route", "search")
                                                 .AddQueryParam("term", title.ToLower().Trim())
                                                 .Build();

                var httpResponse = _httpClient.Get<List<ShowResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSearchResult);
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook. {1}", ex, title, ex.Message);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook. {1}", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from SkyHook. {1}", ex, title, ex.Message);
            }
        }

        private Game MapSearchResult(ShowResource show)
        {
            var game = _seriesService.FindByIgdbId(show.TvdbId);

            if (game == null)
            {
                game = MapSeries(show);
            }

            return game;
        }

        private Game MapSeries(ShowResource show)
        {
            var game = new Game();
            game.TvdbId = show.TvdbId;

            if (show.MobyGamesId.HasValue)
            {
                game.MobyGamesId = show.MobyGamesId.Value;
            }

            if (show.RawgId.HasValue)
            {
                game.RawgId = show.RawgId.Value;
            }

            if (show.TmdbId.HasValue)
            {
                game.TmdbId = show.TmdbId.Value;
            }

            game.ImdbId = show.ImdbId;
            game.MalIds = show.MalIds;
            game.AniListIds = show.AniListIds;
            game.Title = show.Title;
            game.CleanTitle = Parser.Parser.CleanGameTitle(show.Title);
            game.SortTitle = GameTitleNormalizer.Normalize(show.Title, show.TvdbId);

            game.OriginalLanguage = show.OriginalLanguage.IsNotNullOrWhiteSpace() ?
                IsoLanguages.Find(show.OriginalLanguage.ToLower())?.Language ?? Language.English :
                Language.English;

            if (show.FirstAired != null)
            {
                game.FirstAired = DateTime.ParseExact(show.FirstAired, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                game.Year = game.FirstAired.Value.Year;
            }

            if (show.LastAired != null)
            {
                game.LastAired = DateTime.ParseExact(show.LastAired, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }

            game.Overview = show.Overview;

            if (show.Runtime != null)
            {
                game.Runtime = show.Runtime.Value;
            }

            game.Network = show.Network;

            if (show.TimeOfDay != null)
            {
                game.AirTime = string.Format("{0:00}:{1:00}", show.TimeOfDay.Hours, show.TimeOfDay.Minutes);
            }

            game.TitleSlug = show.Slug;
            game.Status = MapGameStatus(show.Status);
            game.Ratings = MapRatings(show.Rating);
            game.Genres = show.Genres;
            game.OriginalCountry = show.OriginalCountry;

            if (show.ContentRating.IsNotNullOrWhiteSpace())
            {
                game.Certification = show.ContentRating.ToUpper();
            }

            if (_dailyGameService.IsDailySeries(game.TvdbId))
            {
                game.SeriesType = GameTypes.Daily;
            }

            game.Actors = show.Actors.Select(MapActors).ToList();
            game.Platforms = show.Platforms.Select(MapSeason).ToList();
            game.Images = show.Images.Select(MapImage).ToList();
            game.Monitored = true;

            return game;
        }

        private static Actor MapActors(ActorResource arg)
        {
            var newActor = new Actor
            {
                Name = arg.Name,
                Character = arg.Character
            };

            if (arg.Image != null)
            {
                newActor.Images = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover(MediaCoverTypes.Headshot, arg.Image)
                };
            }

            return newActor;
        }

        private static Rom MapEpisode(RomResource oracleEpisode)
        {
            var rom = new Rom();
            rom.TvdbId = oracleEpisode.TvdbId;
            rom.Overview = oracleEpisode.Overview;
            rom.SeasonNumber = oracleEpisode.SeasonNumber;
            rom.EpisodeNumber = oracleEpisode.EpisodeNumber;
            rom.AbsoluteEpisodeNumber = oracleEpisode.AbsoluteEpisodeNumber;
            rom.Title = oracleEpisode.Title;
            rom.AiredAfterPlatformNumber = oracleEpisode.AiredAfterPlatformNumber;
            rom.AiredBeforePlatformNumber = oracleEpisode.AiredBeforePlatformNumber;
            rom.AiredBeforeRomNumber = oracleEpisode.AiredBeforeRomNumber;

            rom.AirDate = oracleEpisode.AirDate;
            rom.AirDateUtc = oracleEpisode.AirDateUtc;
            rom.Runtime = oracleEpisode.Runtime;
            rom.FinaleType = oracleEpisode.FinaleType;

            rom.Ratings = MapRatings(oracleEpisode.Rating);

            // Don't include game fanart images as rom screenshot
            if (oracleEpisode.Image != null)
            {
                rom.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, oracleEpisode.Image));
            }

            return rom;
        }

        private static Platform MapSeason(SeasonResource seasonResource)
        {
            return new Platform
            {
                SeasonNumber = seasonResource.SeasonNumber,
                Images = seasonResource.Images.Select(MapImage).ToList(),
                Monitored = seasonResource.SeasonNumber > 0
            };
        }

        private static GameStatusType MapGameStatus(string status)
        {
            if (status.Equals("ended", StringComparison.InvariantCultureIgnoreCase))
            {
                return GameStatusType.Ended;
            }

            if (status.Equals("upcoming", StringComparison.InvariantCultureIgnoreCase))
            {
                return GameStatusType.Upcoming;
            }

            return GameStatusType.Continuing;
        }

        private static Ratings MapRatings(RatingResource rating)
        {
            if (rating == null)
            {
                return new Ratings();
            }

            return new Ratings
            {
                Votes = rating.Count,
                Value = rating.Value
            };
        }

        private static MediaCover.MediaCover MapImage(ImageResource arg)
        {
            return new MediaCover.MediaCover
            {
                RemoteUrl = arg.Url,
                CoverType = MapCoverType(arg.CoverType)
            };
        }

        private static MediaCoverTypes MapCoverType(string coverType)
        {
            switch (coverType.ToLower())
            {
                case "poster":
                    return MediaCoverTypes.Poster;
                case "banner":
                    return MediaCoverTypes.Banner;
                case "fanart":
                    return MediaCoverTypes.Fanart;
                case "clearlogo":
                    return MediaCoverTypes.Clearlogo;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }
    }
}
