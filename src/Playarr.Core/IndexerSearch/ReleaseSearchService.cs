using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Exceptions;
using Playarr.Core.Indexers;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        Task<List<DownloadDecision>> EpisodeSearch(int romId, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> EpisodeSearch(Rom rom, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> SeasonSearch(int gameId, int platformNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> SeasonSearch(int gameId, int platformNumber, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
    }

    public class ReleaseSearchService : ISearchForReleases
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly ISceneMappingService _sceneMapping;
        private readonly IGameService _seriesService;
        private readonly IRomService _episodeService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public ReleaseSearchService(IIndexerFactory indexerFactory,
                                ISceneMappingService sceneMapping,
                                IGameService seriesService,
                                IRomService episodeService,
                                IMakeDownloadDecision makeDownloadDecision,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _sceneMapping = sceneMapping;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public async Task<List<DownloadDecision>> EpisodeSearch(int romId, bool userInvokedSearch, bool interactiveSearch)
        {
            var rom = _episodeService.GetEpisode(romId);

            return await EpisodeSearch(rom, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> EpisodeSearch(Rom rom, bool userInvokedSearch, bool interactiveSearch)
        {
            var game = _seriesService.GetSeries(rom.SeriesId);

            if (game.SeriesType == GameTypes.Daily)
            {
                if (string.IsNullOrWhiteSpace(rom.AirDate))
                {
                    _logger.Error("Daily rom is missing an air date. Try refreshing the game info.");
                    throw new SearchFailedException("Air date is missing");
                }

                return await SearchDaily(game, rom, false, userInvokedSearch, interactiveSearch);
            }

            if (game.SeriesType == GameTypes.Anime)
            {
                if (rom.SeasonNumber == 0 &&
                    rom.SceneAbsoluteEpisodeNumber == null &&
                    rom.AbsoluteEpisodeNumber == null)
                {
                    // Search for special roms in platform 0 that don't have absolute rom numbers
                    return await SearchSpecial(game, new List<Rom> { rom }, false, userInvokedSearch, interactiveSearch);
                }

                return await SearchAnime(game, rom, false, userInvokedSearch, interactiveSearch);
            }

            if (rom.SeasonNumber == 0)
            {
                // Search for special roms in platform 0
                return await SearchSpecial(game, new List<Rom> { rom }, false, userInvokedSearch, interactiveSearch);
            }

            return await SearchSingle(game, rom, false, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> SeasonSearch(int gameId, int platformNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var roms = _episodeService.GetEpisodesBySeason(gameId, platformNumber);

            if (missingOnly)
            {
                roms = roms.Where(e => !e.HasFile).ToList();
            }

            if (roms.Count == 0)
            {
                _logger.Debug("No wanted roms found for platform {0}", platformNumber);
                return new List<DownloadDecision>();
            }

            return await SeasonSearch(gameId, platformNumber, roms, monitoredOnly, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> SeasonSearch(int gameId, int platformNumber, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var game = _seriesService.GetSeries(gameId);

            if (game.SeriesType == GameTypes.Anime)
            {
                return await SearchAnimeSeason(game, roms, monitoredOnly, userInvokedSearch, interactiveSearch);
            }

            if (game.SeriesType == GameTypes.Daily)
            {
                return await SearchDailySeason(game, roms, monitoredOnly, userInvokedSearch, interactiveSearch);
            }

            var mappings = GetSceneSeasonMappings(game, roms);

            var downloadDecisions = new List<DownloadDecision>();

            foreach (var mapping in mappings)
            {
                if (mapping.SeasonNumber == 0)
                {
                    // search for special roms in platform 0
                    downloadDecisions.AddRange(await SearchSpecial(game, mapping.Roms, monitoredOnly, userInvokedSearch, interactiveSearch));
                    continue;
                }

                if (mapping.Roms.Count == 1)
                {
                    var searchSpec = Get<SingleEpisodeSearchCriteria>(game, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.SeasonNumber = mapping.SeasonNumber;
                    searchSpec.EpisodeNumber = mapping.EpisodeMapping.EpisodeNumber;

                    var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                    downloadDecisions.AddRange(decisions);
                }
                else
                {
                    var searchSpec = Get<SeasonSearchCriteria>(game, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.SeasonNumber = mapping.SeasonNumber;

                    var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                    downloadDecisions.AddRange(decisions);
                }
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private List<SceneSeasonMapping> GetSceneSeasonMappings(Game game, List<Rom> roms)
        {
            var dict = new Dictionary<SceneSeasonMapping, SceneSeasonMapping>();

            var sceneMappings = _sceneMapping.FindByIgdbId(game.TvdbId);

            // Group the rom by SceneSeasonNumber/SeasonNumber, in 99% of cases this will result in 1 groupedEpisode
            var groupedEpisodes = roms.ToLookup(v => ((v.SceneSeasonNumber ?? v.SeasonNumber) * 100000) + v.SeasonNumber);

            foreach (var groupedEpisode in groupedEpisodes)
            {
                var episodeMappings = GetSceneEpisodeMappings(game, groupedEpisode.First(), sceneMappings);

                foreach (var episodeMapping in episodeMappings)
                {
                    var seasonMapping = new SceneSeasonMapping
                    {
                        Roms = groupedEpisode.ToList(),
                        EpisodeMapping = episodeMapping,
                        SceneTitles = episodeMapping.SceneTitles,
                        SearchMode = episodeMapping.SearchMode,
                        SeasonNumber = episodeMapping.SeasonNumber
                    };

                    if (dict.TryGetValue(seasonMapping, out var existing))
                    {
                        existing.Roms.AddRange(seasonMapping.Roms);
                        existing.SceneTitles.AddRange(seasonMapping.SceneTitles);
                    }
                    else
                    {
                        dict[seasonMapping] = seasonMapping;
                    }
                }
            }

            foreach (var item in dict)
            {
                item.Value.Roms = item.Value.Roms.Distinct().ToList();
                item.Value.SceneTitles = item.Value.SceneTitles.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            }

            return dict.Values.ToList();
        }

        private List<SceneEpisodeMapping> GetSceneEpisodeMappings(Game game, Rom rom)
        {
            var dict = new Dictionary<SceneEpisodeMapping, SceneEpisodeMapping>();

            var sceneMappings = _sceneMapping.FindByIgdbId(game.TvdbId);

            var episodeMappings = GetSceneEpisodeMappings(game, rom, sceneMappings);

            foreach (var episodeMapping in episodeMappings)
            {
                if (dict.TryGetValue(episodeMapping, out var existing))
                {
                    existing.SceneTitles.AddRange(episodeMapping.SceneTitles);
                }
                else
                {
                    dict[episodeMapping] = episodeMapping;
                }
            }

            foreach (var item in dict)
            {
                item.Value.SceneTitles = item.Value.SceneTitles.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            }

            return dict.Values.ToList();
        }

        private IEnumerable<SceneEpisodeMapping> GetSceneEpisodeMappings(Game game, Rom rom, List<SceneMapping> sceneMappings)
        {
            var includeGlobal = true;

            foreach (var sceneMapping in sceneMappings)
            {
                // There are two kinds of mappings:
                // - Mapped on Release Platform Number with sceneMapping.SceneSeasonNumber specified and optionally sceneMapping.SeasonNumber. This translates via rom.SceneSeasonNumber/SeasonNumber to specific roms.
                // - Mapped on Rom Platform Number with optionally sceneMapping.SeasonNumber. This translates from rom.SceneSeasonNumber/SeasonNumber to specific releases. (Filter by rom.SeasonNumber or globally)

                var ignoreSceneNumbering = sceneMapping.SceneOrigin == "tvdb" || sceneMapping.SceneOrigin == "unknown:tvdb";
                var mappingScenePlatformNumber = sceneMapping.SceneSeasonNumber.NonNegative();
                var mappingPlatformNumber = sceneMapping.SeasonNumber.NonNegative();

                // Select scene or tvdb on the rom
                var mappedPlatformNumber = ignoreSceneNumbering ? rom.SeasonNumber : (rom.SceneSeasonNumber ?? rom.SeasonNumber);
                var releasePlatformNumber = sceneMapping.SceneSeasonNumber.NonNegative() ?? mappedPlatformNumber;

                if (mappingScenePlatformNumber.HasValue)
                {
                    // Apply the alternative mapping (release to scene/tvdb)
                    var mappedAltPlatformNumber = sceneMapping.SeasonNumber.NonNegative() ?? sceneMapping.SceneSeasonNumber.NonNegative() ?? mappedPlatformNumber;

                    // Check if the mapping applies to the current platform
                    if (mappedAltPlatformNumber != mappedPlatformNumber)
                    {
                        continue;
                    }
                }
                else
                {
                    // Check if the mapping applies to the current platform
                    if (mappingPlatformNumber.HasValue && mappingPlatformNumber.Value != rom.SeasonNumber)
                    {
                        continue;
                    }
                }

                if (sceneMapping.SearchTerm == game.Title && sceneMapping.FilterRegex.IsNullOrWhiteSpace())
                {
                    // Disable the implied mapping if we have an explicit mapping by the same name
                    includeGlobal = false;
                }

                // By default we do a alt title search in case indexers don't have the release properly indexed.  Services can override this behavior.
                var searchMode = sceneMapping.SearchMode ?? ((mappingScenePlatformNumber.HasValue && game.CleanTitle != sceneMapping.SearchTerm.CleanGameTitle()) ? SearchMode.SearchTitle : SearchMode.Default);

                if (ignoreSceneNumbering)
                {
                    yield return new SceneEpisodeMapping
                    {
                        Rom = rom,
                        SearchMode = searchMode,
                        SceneTitles = new List<string> { sceneMapping.SearchTerm },
                        SeasonNumber = releasePlatformNumber,
                        EpisodeNumber = rom.EpisodeNumber,
                        AbsoluteEpisodeNumber = rom.AbsoluteEpisodeNumber
                    };
                }
                else
                {
                    yield return new SceneEpisodeMapping
                    {
                        Rom = rom,
                        SearchMode = searchMode,
                        SceneTitles = new List<string> { sceneMapping.SearchTerm },
                        SeasonNumber = releasePlatformNumber,
                        EpisodeNumber = rom.SceneEpisodeNumber ?? rom.EpisodeNumber,
                        AbsoluteEpisodeNumber = rom.SceneAbsoluteEpisodeNumber ?? rom.AbsoluteEpisodeNumber
                    };
                }
            }

            if (includeGlobal)
            {
                yield return new SceneEpisodeMapping
                {
                    Rom = rom,
                    SearchMode = SearchMode.Default,
                    SceneTitles = new List<string> { game.Title },
                    SeasonNumber = rom.SceneSeasonNumber ?? rom.SeasonNumber,
                    EpisodeNumber = rom.SceneEpisodeNumber ?? rom.EpisodeNumber,
                    AbsoluteEpisodeNumber = rom.SceneSeasonNumber ?? rom.AbsoluteEpisodeNumber
                };
            }
        }

        private async Task<List<DownloadDecision>> SearchSingle(Game game, Rom rom, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var mappings = GetSceneEpisodeMappings(game, rom);

            var downloadDecisions = new List<DownloadDecision>();

            foreach (var mapping in mappings)
            {
                var searchSpec = Get<SingleEpisodeSearchCriteria>(game, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                searchSpec.SeasonNumber = mapping.SeasonNumber;
                searchSpec.EpisodeNumber = mapping.EpisodeNumber;

                var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchDaily(Game game, Rom rom, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var airDate = DateTime.ParseExact(rom.AirDate, Rom.AIR_DATE_FORMAT, CultureInfo.InvariantCulture);
            var searchSpec = Get<DailyEpisodeSearchCriteria>(game, new List<Rom> { rom }, monitoredOnly, userInvokedSearch, interactiveSearch);
            searchSpec.AirDate = airDate;

            var downloadDecisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchAnime(Game game, Rom rom, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch, bool isSeasonSearch = false)
        {
            var searchSpec = Get<AnimeEpisodeSearchCriteria>(game, new List<Rom> { rom }, monitoredOnly, userInvokedSearch, interactiveSearch);

            searchSpec.IsSeasonSearch = isSeasonSearch;

            searchSpec.SeasonNumber = rom.SceneSeasonNumber ?? rom.SeasonNumber;
            searchSpec.EpisodeNumber = rom.SceneEpisodeNumber ?? rom.EpisodeNumber;
            searchSpec.AbsoluteEpisodeNumber = rom.SceneAbsoluteEpisodeNumber ?? rom.AbsoluteEpisodeNumber ?? 0;

            var downloadDecisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchSpecial(Game game, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<SpecialEpisodeSearchCriteria>(game, roms, monitoredOnly, userInvokedSearch, interactiveSearch);

            // build list of queries for each rom in the form: "<game> <rom-title>"
            searchSpec.EpisodeQueryTitles = roms.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                                                    .Where(e => interactiveSearch || !monitoredOnly || e.Monitored)
                                                    .SelectMany(e => searchSpec.CleanSceneTitles.Select(title => title + " " + SearchCriteriaBase.GetCleanSceneTitle(e.Title)))
                                                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                                    .ToArray();

            downloadDecisions.AddRange(await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));

            // Search for each rom by platform/rom number as well
            foreach (var rom in roms)
            {
                // Rom needs to be monitored if it's not an interactive search
                if (!interactiveSearch && monitoredOnly && !rom.Monitored)
                {
                    continue;
                }

                downloadDecisions.AddRange(await SearchSingle(game, rom, monitoredOnly, userInvokedSearch, interactiveSearch));
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchAnimeSeason(Game game, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<AnimeSeasonSearchCriteria>(game, roms, monitoredOnly, userInvokedSearch, interactiveSearch);

            // Rom needs to be monitored if it's not an interactive search
            // and Ensure rom has an airdate and has already aired
            var episodesToSearch = roms
                .Where(ep => interactiveSearch || !monitoredOnly || ep.Monitored)
                .Where(ep => ep.AirDateUtc.HasValue && ep.AirDateUtc.Value.Before(DateTime.UtcNow))
                .ToList();

            var seasonsToSearch = GetSceneSeasonMappings(game, episodesToSearch)
                .GroupBy(ep => ep.SeasonNumber)
                .Select(epList => epList.First())
                .ToList();

            foreach (var platform in seasonsToSearch)
            {
                searchSpec.SeasonNumber = platform.SeasonNumber;

                var decisions = await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }

            foreach (var rom in episodesToSearch)
            {
                downloadDecisions.AddRange(await SearchAnime(game, rom, monitoredOnly, userInvokedSearch, interactiveSearch, true));
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private async Task<List<DownloadDecision>> SearchDailySeason(Game game, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            // Rom needs to be monitored if it's not an interactive search
            // and Ensure rom has an airdate
            var episodesToSearch = roms
                .Where(ep => interactiveSearch || !monitoredOnly || ep.Monitored)
                .Where(ep => ep.AirDate.IsNotNullOrWhiteSpace())
                .ToList();

            foreach (var yearGroup in episodesToSearch.GroupBy(v => DateTime.ParseExact(v.AirDate, Rom.AIR_DATE_FORMAT, CultureInfo.InvariantCulture).Year))
            {
                var yearEpisodes = yearGroup.ToList();

                if (yearEpisodes.Count > 1)
                {
                    var searchSpec = Get<DailySeasonSearchCriteria>(game, yearEpisodes, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.Year = yearGroup.Key;

                    downloadDecisions.AddRange(await Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));
                }
                else
                {
                    downloadDecisions.AddRange(await SearchDaily(game, yearEpisodes.First(), monitoredOnly, userInvokedSearch, interactiveSearch));
                }
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private TSpec Get<TSpec>(Game game, List<Rom> roms, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Game = game;
            spec.SceneTitles = _sceneMapping.GetSceneNames(game.TvdbId,
                                                           roms.Select(e => e.SeasonNumber).Distinct().ToList(),
                                                           roms.Select(e => e.SceneSeasonNumber ?? e.SeasonNumber).Distinct().ToList());

            spec.Roms = roms;
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            if (!spec.SceneTitles.Contains(game.Title, StringComparer.InvariantCultureIgnoreCase))
            {
                spec.SceneTitles.Add(game.Title);
            }

            return spec;
        }

        private TSpec Get<TSpec>(Game game, SceneEpisodeMapping mapping, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Game = game;
            spec.SceneTitles = mapping.SceneTitles;
            spec.SearchMode = mapping.SearchMode;

            spec.Roms = new List<Rom> { mapping.Rom };
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private TSpec Get<TSpec>(Game game, SceneSeasonMapping mapping, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Game = game;
            spec.SceneTitles = mapping.SceneTitles;
            spec.SearchMode = mapping.SearchMode;

            spec.Roms = mapping.Roms;
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private async Task<List<DownloadDecision>> Dispatch(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Game.Tags).Any()).ToList();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

            var tasks = indexers.Select(indexer => DispatchIndexer(searchAction, indexer, criteriaBase));

            var batch = await Task.WhenAll(tasks);

            var reports = batch.SelectMany(x => x).ToList();

            _logger.ProgressDebug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            // Update the last search time for all roms if at least 1 indexer was searched.
            if (indexers.Any())
            {
                var lastSearchTime = DateTime.UtcNow;
                _logger.Debug("Setting last search time to: {0}", lastSearchTime);

                criteriaBase.Roms.ForEach(e => e.LastSearchTime = lastSearchTime);
                _episodeService.UpdateLastSearchTime(criteriaBase.Roms);
            }

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }

        private async Task<IList<ReleaseInfo>> DispatchIndexer(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, IIndexer indexer, SearchCriteriaBase criteriaBase)
        {
            try
            {
                return await searchAction(indexer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while searching for {0}", criteriaBase);
            }

            return Array.Empty<ReleaseInfo>();
        }

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections and higher indexer priority.
            return decisions.GroupBy(d => d.RemoteEpisode.Release.Guid)
                .Select(d => d.OrderBy(v => v.Rejections.Count()).ThenBy(v => v.RemoteEpisode?.Release?.IndexerPriority ?? IndexerDefinition.DefaultPriority).First())
                .ToList();
        }
    }
}
