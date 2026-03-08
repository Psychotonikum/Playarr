using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Parser
{
    public interface IParsingService
    {
        Game GetSeries(string title);
        RemoteEpisode Map(ParsedRomInfo parsedRomInfo, int igdbId, int mobyGamesId, string imdbId, SearchCriteriaBase searchCriteria = null);
        RemoteEpisode Map(ParsedRomInfo parsedRomInfo, Game game);
        RemoteEpisode Map(ParsedRomInfo parsedRomInfo, int gameId, IEnumerable<int> romIds);
        List<Rom> GetEpisodes(ParsedRomInfo parsedRomInfo, Game game, bool sceneSource, SearchCriteriaBase searchCriteria = null);
        ParsedRomInfo ParseSpecialRomTitle(ParsedRomInfo parsedRomInfo, string releaseTitle, int igdbId, int mobyGamesId, string imdbId, SearchCriteriaBase searchCriteria = null);
        ParsedRomInfo ParseSpecialRomTitle(ParsedRomInfo parsedRomInfo, string releaseTitle, Game game);
    }

    public class ParsingService : IParsingService
    {
        private readonly IRomService _episodeService;
        private readonly IGameService _seriesService;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly Logger _logger;

        public ParsingService(IRomService episodeService,
                              IGameService seriesService,
                              ISceneMappingService sceneMappingService,
                              Logger logger)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _sceneMappingService = sceneMappingService;
            _logger = logger;
        }

        public Game GetSeries(string title)
        {
            var parsedRomInfo = Parser.ParseTitle(title);

            if (parsedRomInfo == null)
            {
                return _seriesService.FindByTitle(title);
            }

            var igdbId = _sceneMappingService.FindIgdbId(parsedRomInfo.GameTitle, parsedRomInfo.ReleaseTitle, parsedRomInfo.PlatformNumber);

            if (igdbId.HasValue)
            {
                return _seriesService.FindByIgdbId(igdbId.Value);
            }

            var game = _seriesService.FindByTitle(parsedRomInfo.GameTitle);

            if (game == null && parsedRomInfo.GameTitleInfo.AllTitles != null)
            {
                game = GetSeriesByAllTitles(parsedRomInfo);
            }

            if (game == null)
            {
                game = _seriesService.FindByTitle(parsedRomInfo.GameTitleInfo.TitleWithoutYear,
                                                    parsedRomInfo.GameTitleInfo.Year);
            }

            return game;
        }

        private Game GetSeriesByAllTitles(ParsedRomInfo parsedRomInfo)
        {
            var year = parsedRomInfo.GameTitleInfo.Year;
            Game foundSeries = null;
            int? foundIgdbId = null;

            // Match each title individually, they must all resolve to the same igdbid
            foreach (var title in parsedRomInfo.GameTitleInfo.AllTitles)
            {
                Game game = null;

                if (year > 0)
                {
                    game = _seriesService.FindByTitle(title, year);

                    // Fall back to title + year being part of the title, this will allow
                    // matching game with the same name that include the year in the title.
                    if (game == null)
                    {
                        game = _seriesService.FindByTitle($"{title} {year}");
                    }
                }
                else
                {
                    game = _seriesService.FindByTitle(title);
                }

                var igdbId = game?.IgdbId;

                if (game == null)
                {
                    igdbId = _sceneMappingService.FindIgdbId(title, parsedRomInfo.ReleaseTitle, parsedRomInfo.PlatformNumber);
                }

                if (!igdbId.HasValue)
                {
                    _logger.Trace("Title {0} not matching any game.", title);
                    continue;
                }

                if (foundIgdbId.HasValue && igdbId != foundIgdbId)
                {
                    _logger.Trace("Title {0} both matches igdbid {1} and {2}, no game selected.", parsedRomInfo.GameTitle, foundIgdbId, igdbId);
                    return null;
                }

                if (foundSeries == null)
                {
                    foundSeries = game;
                }

                foundIgdbId = igdbId;
            }

            if (foundSeries == null && foundIgdbId.HasValue)
            {
                foundSeries = _seriesService.FindByIgdbId(foundIgdbId.Value);
            }

            return foundSeries;
        }

        private Game GetSeriesAliasTitleAndYear(ParsedRomInfo parsedRomInfo)
        {
            var year = parsedRomInfo.GameTitleInfo.Year;
            var titleWithoutyear = parsedRomInfo.GameTitleInfo.TitleWithoutYear;
            var igdbId = _sceneMappingService.FindIgdbId(titleWithoutyear, parsedRomInfo.ReleaseTitle, parsedRomInfo.PlatformNumber);

            if (igdbId.HasValue)
            {
                var game = _seriesService.FindByIgdbId(igdbId.Value);

                if (game.Year == year)
                {
                    return game;
                }
            }

            return null;
        }

        public RemoteEpisode Map(ParsedRomInfo parsedRomInfo, int igdbId, int mobyGamesId, string imdbId, SearchCriteriaBase searchCriteria = null)
        {
            return Map(parsedRomInfo, igdbId, mobyGamesId, imdbId, null, searchCriteria);
        }

        public RemoteEpisode Map(ParsedRomInfo parsedRomInfo, Game game)
        {
            return Map(parsedRomInfo, 0, 0, null, game, null);
        }

        public RemoteEpisode Map(ParsedRomInfo parsedRomInfo, int gameId, IEnumerable<int> romIds)
        {
            return new RemoteEpisode
                   {
                       ParsedRomInfo = parsedRomInfo,
                       Game = _seriesService.GetSeries(gameId),
                       Roms = _episodeService.GetEpisodes(romIds)
                   };
        }

        private RemoteEpisode Map(ParsedRomInfo parsedRomInfo, int igdbId, int mobyGamesId, string imdbId, Game game, SearchCriteriaBase searchCriteria)
        {
            var sceneMapping = _sceneMappingService.FindSceneMapping(parsedRomInfo.GameTitle, parsedRomInfo.ReleaseTitle, parsedRomInfo.PlatformNumber);

            var remoteRom = new RemoteEpisode
            {
                ParsedRomInfo = parsedRomInfo,
                SceneMapping = sceneMapping,
                MappedPlatformNumber = parsedRomInfo.PlatformNumber
            };

            // For now we just detect igdb vs scene, but we can do multiple 'origins' in the future.
            var sceneSource = true;
            if (sceneMapping != null)
            {
                if (sceneMapping.PlatformNumber.HasValue && sceneMapping.PlatformNumber.Value >= 0 &&
                    sceneMapping.ScenePlatformNumber <= parsedRomInfo.PlatformNumber)
                {
                    remoteRom.MappedPlatformNumber += sceneMapping.PlatformNumber.Value - sceneMapping.ScenePlatformNumber.Value;
                }

                if (sceneMapping.SceneOrigin == "igdb")
                {
                    sceneSource = false;
                }
                else if (sceneMapping.Type == "XemService" &&
                         sceneMapping.ScenePlatformNumber.NonNegative().HasValue &&
                         parsedRomInfo.PlatformNumber == 1 &&
                         sceneMapping.ScenePlatformNumber != parsedRomInfo.PlatformNumber)
                {
                    remoteRom.MappedPlatformNumber = sceneMapping.ScenePlatformNumber.Value;
                }
            }

            if (game == null)
            {
                var seriesMatch = FindSeries(parsedRomInfo, igdbId, mobyGamesId, imdbId, sceneMapping, searchCriteria);

                if (seriesMatch != null)
                {
                    game = seriesMatch.Game;
                    remoteRom.SeriesMatchType = seriesMatch.MatchType;
                }
            }

            if (game != null)
            {
                remoteRom.Game = game;

                if (ValidateParsedRomInfo.ValidateForGameType(parsedRomInfo, game))
                {
                    remoteRom.Roms = GetEpisodes(parsedRomInfo, game, remoteRom.MappedPlatformNumber, sceneSource, searchCriteria);
                }
            }

            remoteRom.Languages = parsedRomInfo.Languages;

            if (remoteRom.Roms == null)
            {
                remoteRom.Roms = new List<Rom>();
            }

            if (searchCriteria != null)
            {
                var requestedEpisodes = searchCriteria.Roms.ToDictionaryIgnoreDuplicates(v => v.Id);
                remoteRom.EpisodeRequested = remoteRom.Roms.Any(v => requestedEpisodes.ContainsKey(v.Id));
            }

            return remoteRom;
        }

        public List<Rom> GetEpisodes(ParsedRomInfo parsedRomInfo, Game game, bool sceneSource, SearchCriteriaBase searchCriteria = null)
        {
            if (sceneSource)
            {
                var remoteRom = Map(parsedRomInfo, 0, 0, null, game, searchCriteria);

                return remoteRom.Roms;
            }

            return GetEpisodes(parsedRomInfo, game, parsedRomInfo.PlatformNumber, sceneSource, searchCriteria);
        }

        private List<Rom> GetEpisodes(ParsedRomInfo parsedRomInfo, Game game, int mappedPlatformNumber, bool sceneSource, SearchCriteriaBase searchCriteria)
        {
            if (parsedRomInfo.FullSeason)
            {
                if (game.UseSceneNumbering && sceneSource)
                {
                    var roms = _episodeService.GetEpisodesBySceneSeason(game.Id, mappedPlatformNumber);

                    // If roms were found by the scene platform number return them, otherwise fallback to look-up by platform number
                    if (roms.Any())
                    {
                        return roms;
                    }
                }

                return _episodeService.GetEpisodesBySeason(game.Id, mappedPlatformNumber);
            }

            if (parsedRomInfo.IsDaily)
            {
                var romInfo = GetDailyEpisode(game, parsedRomInfo.AirDate, parsedRomInfo.DailyPart, searchCriteria);

                if (romInfo != null)
                {
                    return new List<Rom> { romInfo };
                }

                return new List<Rom>();
            }

            if (parsedRomInfo.IsAbsoluteNumbering)
            {
                return GetAnimeEpisodes(game, parsedRomInfo, mappedPlatformNumber, sceneSource, searchCriteria);
            }

            if (parsedRomInfo.IsPossibleSceneSeasonSpecial)
            {
                var parsedSpecialRomInfo = ParseSpecialRomTitle(parsedRomInfo, parsedRomInfo.ReleaseTitle, game);

                if (parsedSpecialRomInfo != null)
                {
                    // Use the platform number and disable scene source since the platform/rom numbers that were returned are not scene numbers
                    return GetStandardEpisodes(game, parsedSpecialRomInfo, parsedSpecialRomInfo.PlatformNumber, false, searchCriteria);
                }
            }

            if (parsedRomInfo.Special && mappedPlatformNumber != 0)
            {
                return new List<Rom>();
            }

            return GetStandardEpisodes(game, parsedRomInfo, mappedPlatformNumber, sceneSource, searchCriteria);
        }

        public ParsedRomInfo ParseSpecialRomTitle(ParsedRomInfo parsedRomInfo, string releaseTitle, int igdbId, int mobyGamesId, string imdbId, SearchCriteriaBase searchCriteria = null)
        {
            if (searchCriteria != null)
            {
                if (igdbId != 0 && igdbId == searchCriteria.Game.IgdbId)
                {
                    return ParseSpecialRomTitle(parsedRomInfo, releaseTitle, searchCriteria.Game);
                }

                if (mobyGamesId != 0 && mobyGamesId == searchCriteria.Game.MobyGamesId)
                {
                    return ParseSpecialRomTitle(parsedRomInfo, releaseTitle, searchCriteria.Game);
                }

                if (imdbId.IsNotNullOrWhiteSpace() && imdbId.Equals(searchCriteria.Game.ImdbId, StringComparison.Ordinal))
                {
                    return ParseSpecialRomTitle(parsedRomInfo, releaseTitle, searchCriteria.Game);
                }
            }

            var game = GetSeries(releaseTitle);

            if (game == null)
            {
                game = _seriesService.FindByTitleInexact(releaseTitle);
            }

            if (game == null && igdbId > 0)
            {
                game = _seriesService.FindByIgdbId(igdbId);
            }

            if (game == null && mobyGamesId > 0)
            {
                game = _seriesService.FindByMobyGamesId(mobyGamesId);
            }

            if (game == null && imdbId.IsNotNullOrWhiteSpace())
            {
                game = _seriesService.FindByImdbId(imdbId);
            }

            if (game == null)
            {
                _logger.Debug("No matching game {0}", releaseTitle);
                return null;
            }

            return ParseSpecialRomTitle(parsedRomInfo, releaseTitle, game);
        }

        public ParsedRomInfo ParseSpecialRomTitle(ParsedRomInfo parsedRomInfo, string releaseTitle, Game game)
        {
            // SxxE00 roms are sometimes mapped via TheXEM, don't use rom title parsing in that case.
            if (parsedRomInfo != null && parsedRomInfo.IsPossibleSceneSeasonSpecial && game.UseSceneNumbering)
            {
                if (_episodeService.FindEpisodesBySceneNumbering(game.Id, parsedRomInfo.PlatformNumber, 0).Any())
                {
                    return parsedRomInfo;
                }
            }

            // find special rom in game platform 0
            var rom = _episodeService.FindEpisodeByTitle(game.Id, 0, releaseTitle);

            if (rom != null)
            {
                // create parsed info from tv rom
                var info = new ParsedRomInfo
                {
                    ReleaseTitle = releaseTitle,
                    GameTitle = game.Title,
                    GameTitleInfo = new GameTitleInfo
                        {
                            Title = game.Title
                        },
                    PlatformNumber = rom.PlatformNumber,
                    RomNumbers = new int[1] { rom.EpisodeNumber },
                    FullSeason = false,
                    Quality = QualityParser.ParseQuality(releaseTitle),
                    ReleaseGroup = ReleaseGroupParser.ParseReleaseGroup(releaseTitle),
                    Languages = LanguageParser.ParseLanguages(releaseTitle),
                    Special = true
                };

                _logger.Debug("Found special rom {0} for title '{1}'", info, releaseTitle);
                return info;
            }

            return null;
        }

        private FindSeriesResult FindSeries(ParsedRomInfo parsedRomInfo, int igdbId, int mobyGamesId, string imdbId, SceneMapping sceneMapping, SearchCriteriaBase searchCriteria)
        {
            Game game = null;

            if (sceneMapping != null)
            {
                if (searchCriteria != null && searchCriteria.Game.IgdbId == sceneMapping.IgdbId)
                {
                    return new FindSeriesResult(searchCriteria.Game, SeriesMatchType.Alias);
                }

                game = _seriesService.FindByIgdbId(sceneMapping.IgdbId);

                if (game == null)
                {
                    _logger.Debug("No matching game {0}", parsedRomInfo.GameTitle);
                    return null;
                }

                return new FindSeriesResult(game, SeriesMatchType.Alias);
            }

            if (searchCriteria != null)
            {
                if (searchCriteria.Game.CleanTitle == parsedRomInfo.GameTitle.CleanGameTitle())
                {
                    return new FindSeriesResult(searchCriteria.Game, SeriesMatchType.Title);
                }

                if (igdbId > 0 && igdbId == searchCriteria.Game.IgdbId)
                {
                    _logger.ForDebugEvent()
                           .Message("Found matching game by IGDB ID {0}, an alias may be needed for: {1}", igdbId, parsedRomInfo.GameTitle)
                           .Property("IgdbId", igdbId)
                           .Property("ParsedRomInfo", parsedRomInfo)
                           .WriteSentryWarn("IgdbIdMatch", igdbId.ToString(), parsedRomInfo.GameTitle)
                           .Log();

                    return new FindSeriesResult(searchCriteria.Game, SeriesMatchType.Id);
                }

                if (mobyGamesId > 0 && mobyGamesId == searchCriteria.Game.MobyGamesId && igdbId <= 0)
                {
                    _logger.ForDebugEvent()
                           .Message("Found matching game by TVRage ID {0}, an alias may be needed for: {1}", mobyGamesId, parsedRomInfo.GameTitle)
                           .Property("MobyGamesId", mobyGamesId)
                           .Property("ParsedRomInfo", parsedRomInfo)
                           .WriteSentryWarn("MobyGamesIdMatch", mobyGamesId.ToString(), parsedRomInfo.GameTitle)
                           .Log();

                    return new FindSeriesResult(searchCriteria.Game, SeriesMatchType.Id);
                }

                if (imdbId.IsNotNullOrWhiteSpace() && imdbId.Equals(searchCriteria.Game.ImdbId, StringComparison.Ordinal) && igdbId <= 0)
                {
                    _logger.ForDebugEvent()
                           .Message("Found matching game by IMDb ID {0}, an alias may be needed for: {1}", imdbId, parsedRomInfo.GameTitle)
                           .Property("ImdbId", imdbId)
                           .Property("ParsedRomInfo", parsedRomInfo)
                           .WriteSentryWarn("ImdbIdMatch", imdbId, parsedRomInfo.GameTitle)
                           .Log();

                    return new FindSeriesResult(searchCriteria.Game, SeriesMatchType.Id);
                }
            }

            var matchType = SeriesMatchType.Unknown;
            game = _seriesService.FindByTitle(parsedRomInfo.GameTitle);

            if (game != null)
            {
                matchType = SeriesMatchType.Title;
            }

            if (game == null && parsedRomInfo.GameTitleInfo.AllTitles != null)
            {
                game = GetSeriesByAllTitles(parsedRomInfo);
                matchType = SeriesMatchType.Title;
            }

            if (game == null && parsedRomInfo.GameTitleInfo.Year > 0)
            {
                game = _seriesService.FindByTitle(parsedRomInfo.GameTitleInfo.TitleWithoutYear, parsedRomInfo.GameTitleInfo.Year);
                matchType = SeriesMatchType.Title;

                if (game == null)
                {
                    game = GetSeriesAliasTitleAndYear(parsedRomInfo);
                    matchType = SeriesMatchType.Alias;
                }
            }

            if (game == null && igdbId > 0)
            {
                game = _seriesService.FindByIgdbId(igdbId);

                if (game != null)
                {
                    _logger.ForDebugEvent()
                           .Message("Found matching game by IGDB ID {0}, an alias may be needed for: {1}", igdbId, parsedRomInfo.GameTitle)
                           .Property("IgdbId", igdbId)
                           .Property("ParsedRomInfo", parsedRomInfo)
                           .WriteSentryWarn("IgdbIdMatch", igdbId.ToString(), parsedRomInfo.GameTitle)
                           .Log();

                    matchType = SeriesMatchType.Id;
                }
            }

            if (game == null && mobyGamesId > 0 && igdbId <= 0)
            {
                game = _seriesService.FindByMobyGamesId(mobyGamesId);

                if (game != null)
                {
                    _logger.ForDebugEvent()
                           .Message("Found matching game by TVRage ID {0}, an alias may be needed for: {1}", mobyGamesId, parsedRomInfo.GameTitle)
                           .Property("MobyGamesId", mobyGamesId)
                           .Property("ParsedRomInfo", parsedRomInfo)
                           .WriteSentryWarn("MobyGamesIdMatch", mobyGamesId.ToString(), parsedRomInfo.GameTitle)
                           .Log();

                    matchType = SeriesMatchType.Id;
                }
            }

            if (game == null && imdbId.IsNotNullOrWhiteSpace() && igdbId <= 0)
            {
                game = _seriesService.FindByImdbId(imdbId);

                if (game != null)
                {
                    _logger.ForDebugEvent()
                           .Message("Found matching game by IMDb ID {0}, an alias may be needed for: {1}", imdbId, parsedRomInfo.GameTitle)
                           .Property("ImdbId", imdbId)
                           .Property("ParsedRomInfo", parsedRomInfo)
                           .WriteSentryWarn("ImdbIdMatch", imdbId, parsedRomInfo.GameTitle)
                           .Log();

                    matchType = SeriesMatchType.Id;
                }
            }

            if (game == null)
            {
                _logger.Debug("No matching game {0}", parsedRomInfo.GameTitle);
                return null;
            }

            return new FindSeriesResult(game, matchType);
        }

        private Rom GetDailyEpisode(Game game, string airDate, int? part, SearchCriteriaBase searchCriteria)
        {
            Rom romInfo = null;

            if (searchCriteria != null)
            {
                romInfo = searchCriteria.Roms.SingleOrDefault(
                    e => e.AirDate == airDate);
            }

            if (romInfo == null)
            {
                romInfo = _episodeService.FindEpisode(game.Id, airDate, part);
            }

            return romInfo;
        }

        private List<Rom> GetAnimeEpisodes(Game game, ParsedRomInfo parsedRomInfo, int platformNumber, bool sceneSource, SearchCriteriaBase searchCriteria)
        {
            var result = new List<Rom>();

            var scenePlatformNumber = _sceneMappingService.GetScenePlatformNumber(parsedRomInfo.GameTitle, parsedRomInfo.ReleaseTitle);

            foreach (var absoluteRomNumber in parsedRomInfo.AbsoluteRomNumbers)
            {
                var roms = new List<Rom>();

                if (parsedRomInfo.Special)
                {
                    var rom = _episodeService.FindEpisode(game.Id, 0, absoluteRomNumber);
                    roms.AddIfNotNull(rom);
                }
                else if (sceneSource)
                {
                    // Is there a reason why we excluded platform 1 from this handling before?
                    // Might have something to do with the scene name to platform number check
                    // If this needs to be reverted tests will need to be added
                    if (scenePlatformNumber.HasValue)
                    {
                        roms = _episodeService.FindEpisodesBySceneNumbering(game.Id, scenePlatformNumber.Value, absoluteRomNumber);

                        if (roms.Empty())
                        {
                            var rom = _episodeService.FindEpisode(game.Id, scenePlatformNumber.Value, absoluteRomNumber);
                            roms.AddIfNotNull(rom);
                        }
                    }
                    else if (parsedRomInfo.PlatformNumber > 1 && parsedRomInfo.RomNumbers.Empty())
                    {
                        roms = _episodeService.FindEpisodesBySceneNumbering(game.Id, parsedRomInfo.PlatformNumber, absoluteRomNumber);

                        if (roms.Empty())
                        {
                            var rom = _episodeService.FindEpisode(game.Id, parsedRomInfo.PlatformNumber, absoluteRomNumber);
                            roms.AddIfNotNull(rom);
                        }
                    }
                    else
                    {
                        roms = _episodeService.FindEpisodesBySceneNumbering(game.Id, absoluteRomNumber);

                        // Don't allow multiple results without a scene name mapping.
                        if (roms.Count > 1)
                        {
                            roms.Clear();
                        }
                    }
                }

                if (roms.Empty())
                {
                    var rom = _episodeService.FindEpisode(game.Id, absoluteRomNumber);
                    roms.AddIfNotNull(rom);
                }

                foreach (var rom in roms)
                {
                    _logger.Debug("Using absolute rom number {0} for: {1} - IGDB: {2}x{3:00}",
                                absoluteRomNumber,
                                game.Title,
                                rom.PlatformNumber,
                                rom.EpisodeNumber);

                    result.Add(rom);
                }
            }

            return result;
        }

        private List<Rom> GetStandardEpisodes(Game game, ParsedRomInfo parsedRomInfo, int mappedPlatformNumber, bool sceneSource, SearchCriteriaBase searchCriteria)
        {
            var result = new List<Rom>();

            if (parsedRomInfo.RomNumbers == null)
            {
                return new List<Rom>();
            }

            foreach (var romNumber in parsedRomInfo.RomNumbers)
            {
                if (game.UseSceneNumbering && sceneSource)
                {
                    var roms = new List<Rom>();

                    if (searchCriteria != null)
                    {
                        roms = searchCriteria.Roms.Where(e => e.ScenePlatformNumber == parsedRomInfo.PlatformNumber &&
                                                                      e.SceneEpisodeNumber == romNumber).ToList();
                    }

                    if (!roms.Any())
                    {
                        roms = _episodeService.FindEpisodesBySceneNumbering(game.Id, mappedPlatformNumber, romNumber);
                    }

                    if (roms != null && roms.Any())
                    {
                        _logger.Debug("Using Scene to IGDB Mapping for: {0} - Scene: {1}x{2:00} - IGDB: {3}",
                                    game.Title,
                                    roms.First().ScenePlatformNumber,
                                    roms.First().SceneEpisodeNumber,
                                    string.Join(", ", roms.Select(e => string.Format("{0}x{1:00}", e.PlatformNumber, e.EpisodeNumber))));

                        result.AddRange(roms);
                        continue;
                    }
                }

                Rom romInfo = null;

                if (searchCriteria != null)
                {
                    romInfo = searchCriteria.Roms.SingleOrDefault(e => e.PlatformNumber == mappedPlatformNumber && e.EpisodeNumber == romNumber);
                }

                if (romInfo == null)
                {
                    romInfo = _episodeService.FindEpisode(game.Id, mappedPlatformNumber, romNumber);
                }

                if (romInfo != null)
                {
                    result.Add(romInfo);
                }
                else
                {
                    _logger.Debug("Unable to find {0}", parsedRomInfo);
                }
            }

            return result;
        }
    }
}
