using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Common.Instrumentation;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;

namespace Playarr.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly Logger _logger;
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public ProviderDefinition Definition { get; set; }
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _logger = PlayarrLogger.GetLogger(GetType());
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        // Used for anime
        private bool SupportsSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedSearchParameters != null &&
                       capabilities.SupportedSearchParameters.Contains("q");
            }
        }

        // Used for standard/daily
        private bool SupportsTvQuerySearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("q");
            }
        }

        // Used for standard/daily
        private bool SupportsTvTitleSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("title");
            }
        }

        // Combines 'SupportsTvQuerySearch' and 'SupportsTvTitleSearch'
        private bool SupportsTvTextSearches
        {
            get
            {
                return SupportsTvQuerySearch || SupportsTvTitleSearch;
            }
        }

        private bool SupportsIgdbSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("igdbid");
            }
        }

        private bool SupportsImdbSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("imdbid");
            }
        }

        private bool SupportsTvRageSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("rid");
            }
        }

        private bool SupportsTvMazeSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("tvmazeid");
            }
        }

        private bool SupportsTmdbSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("tmdbid");
            }
        }

        // Combines all ID based searches
        private bool SupportsTvIdSearches
        {
            get
            {
                return SupportsIgdbSearch || SupportsImdbSearch || SupportsTvRageSearch || SupportsTvMazeSearch || SupportsTmdbSearch;
            }
        }

        private bool SupportsSeasonSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("platform");
            }
        }

        private bool SupportsEpisodeSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("platform") &&
                       capabilities.SupportedTvSearchParameters.Contains("ep");
            }
        }

        private bool SupportsAggregatedIdSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportsAggregateIdSearch;
            }
        }

        private string TextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.TextSearchEngine;
            }
        }

        private string TvTextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.TvTextSearchEngine;
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            if (capabilities.SupportedTvSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "tvsearch", ""));
            }
            else if (capabilities.SupportedSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.AnimeCategories, "search", ""));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsEpisodeSearch)
            {
                _logger.Debug("Indexer capabilities lacking platform & ep query parameters, no Standard game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (!SupportsTvTextSearches && !SupportsTvIdSearches)
            {
                _logger.Debug("Indexer capabilities lacking q, title, igdbid, imdbid, rid and tvmazeid parameters, no Standard game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            var categories = GetSearchCategories(searchCriteria);

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}&ep={searchCriteria.EpisodeNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}&ep={searchCriteria.EpisodeNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}&ep={searchCriteria.EpisodeNumber}");
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsSeasonSearch)
            {
                _logger.Debug("Indexer capabilities lacking platform query parameter, no Standard game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (!SupportsTvTextSearches && !SupportsTvIdSearches)
            {
                _logger.Debug("Indexer capabilities lacking q, title, igdbid, imdbid, rid and tvmazeid parameters, no Standard game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}");
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsEpisodeSearch)
            {
                _logger.Debug("Indexer capabilities lacking platform & ep query parameters, no Daily game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (!SupportsTvTextSearches && !SupportsTvIdSearches)
            {
                _logger.Debug("Indexer capabilities lacking q, title, igdbid, imdbid, rid and tvmazeid parameters, no Daily game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={searchCriteria.AirDate:yyyy}&ep={searchCriteria.AirDate:MM}/{searchCriteria.AirDate:dd}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={searchCriteria.AirDate:yyyy}&ep={searchCriteria.AirDate:MM}/{searchCriteria.AirDate:dd}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={searchCriteria.AirDate:yyyy}&ep={searchCriteria.AirDate:MM}/{searchCriteria.AirDate:dd}");
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsEpisodeSearch)
            {
                _logger.Debug("Indexer capabilities lacking platform query parameter, no Daily game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (!SupportsTvTextSearches && !SupportsTvIdSearches)
            {
                _logger.Debug("Indexer capabilities lacking q, title, igdbid, imdbid, rid and tvmazeid parameters, no Daily game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={searchCriteria.Year}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={searchCriteria.Year}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    $"&platform={searchCriteria.Year}");
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.AnimeCategories,
                    searchCriteria,
                    $"&q={searchCriteria.AbsoluteEpisodeNumber:00}");

                var includeAnimeStandardFormatSearch = Settings.AnimeStandardFormatSearch &&
                                                       searchCriteria.PlatformNumber > 0 &&
                                                       searchCriteria.EpisodeNumber > 0;

                if (includeAnimeStandardFormatSearch && SupportsEpisodeSearch)
                {
                    AddTvIdPageableRequests(pageableRequests,
                        Settings.AnimeCategories,
                        searchCriteria,
                        $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}&ep={searchCriteria.EpisodeNumber}");
                }

                var queryTitles = TextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;

                foreach (var queryTitle in queryTitles)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.AnimeCategories,
                        "search",
                        $"&q={NewsnabifyTitle(queryTitle)}+{searchCriteria.AbsoluteEpisodeNumber:00}"));

                    if (includeAnimeStandardFormatSearch && SupportsEpisodeSearch)
                    {
                        pageableRequests.Add(GetPagedRequests(MaxPages,
                            Settings.AnimeCategories,
                            "tvsearch",
                            $"&q={NewsnabifyTitle(queryTitle)}&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}&ep={searchCriteria.EpisodeNumber}"));
                    }
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeSeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch && Settings.AnimeStandardFormatSearch && searchCriteria.PlatformNumber > 0)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.AnimeCategories,
                    searchCriteria,
                    $"&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}");

                var queryTitles = TextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;

                foreach (var queryTitle in queryTitles)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.AnimeCategories,
                        "tvsearch",
                        $"&q={NewsnabifyTitle(queryTitle)}&platform={NewznabifyPlatformNumber(searchCriteria.PlatformNumber)}"));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                var categories = GetSearchCategories(searchCriteria);

                foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
                {
                    var query = queryTitle.Replace('+', ' ');
                    query = System.Web.HttpUtility.UrlEncode(query);

                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        categories,
                        "search",
                        $"&q={query}"));
                }
            }

            return pageableRequests;
        }

        private void AddTvIdPageableRequests(IndexerPageableRequestChain chain, IEnumerable<int> categories, SearchCriteriaBase searchCriteria, string parameters)
        {
            var includeIgdbSearch = SupportsIgdbSearch && searchCriteria.Game.IgdbId > 0;
            var includeImdbSearch = SupportsImdbSearch && searchCriteria.Game.ImdbId.IsNotNullOrWhiteSpace();
            var includeTvRageSearch = SupportsTvRageSearch && searchCriteria.Game.MobyGamesId > 0;
            var includeTvMazeSearch = SupportsTvMazeSearch && searchCriteria.Game.RawgId > 0;
            var includeTmdbSearch = SupportsTmdbSearch && searchCriteria.Game.TmdbId > 0;

            if (SupportsAggregatedIdSearch && (includeIgdbSearch || includeTvRageSearch || includeTvMazeSearch || includeTmdbSearch))
            {
                var ids = "";

                if (includeIgdbSearch)
                {
                    ids += "&igdbid=" + searchCriteria.Game.IgdbId;
                }

                if (includeImdbSearch)
                {
                    ids += "&imdbid=" + searchCriteria.Game.ImdbId;
                }

                if (includeTvRageSearch)
                {
                    ids += "&rid=" + searchCriteria.Game.MobyGamesId;
                }

                if (includeTvMazeSearch)
                {
                    ids += "&tvmazeid=" + searchCriteria.Game.RawgId;
                }

                if (includeTmdbSearch)
                {
                    ids += "&tmdbid=" + searchCriteria.Game.TmdbId;
                }

                chain.Add(GetPagedRequests(MaxPages, categories, "tvsearch", ids + parameters));
            }
            else
            {
                if (includeIgdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&igdbid={searchCriteria.Game.IgdbId}{parameters}"));
                }
                else if (includeImdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&imdbid={searchCriteria.Game.ImdbId}{parameters}"));
                }
                else if (includeTvRageSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&rid={searchCriteria.Game.MobyGamesId}{parameters}"));
                }
                else if (includeTvMazeSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&tvmazeid={searchCriteria.Game.RawgId}{parameters}"));
                }
                else if (includeTmdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&tmdbid={searchCriteria.Game.TmdbId}{parameters}"));
                }
            }
        }

        private void AddTitlePageableRequests(IndexerPageableRequestChain chain, IEnumerable<int> categories, SearchCriteriaBase searchCriteria, string parameters)
        {
            if (SupportsTvTitleSearch)
            {
                foreach (var searchTerm in searchCriteria.SceneTitles)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "tvsearch",
                        $"&title={Uri.EscapeDataString(searchTerm)}{parameters}"));
                }
            }
            else if (SupportsTvQuerySearch)
            {
                var queryTitles = TvTextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;
                foreach (var queryTitle in queryTitles)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "tvsearch",
                        $"&q={NewsnabifyTitle(queryTitle)}{parameters}"));
                }
            }
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl =
                $"{Settings.BaseUrl.TrimEnd('/')}{Settings.ApiPath.TrimEnd('/')}?t={searchType}&cat={categoriesQuery}&extended=1{Settings.AdditionalParameters}";

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest($"{baseUrl}{parameters}", HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest($"{baseUrl}&offset={page * PageSize}&limit={PageSize}{parameters}", HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            title = title.Replace("+", " ");
            return Uri.EscapeDataString(title);
        }

        // Temporary workaround for NNTMux considering platform=0 -> null. '00' should work on existing newznab indexers.
        private static string NewznabifyPlatformNumber(int platformNumber)
        {
            return platformNumber == 0 ? "00" : platformNumber.ToString();
        }

        private IList<int> GetSearchCategories(SearchCriteriaBase searchCriteria)
        {
            return searchCriteria.Game?.SeriesType is GameTypes.Anime
                ? Settings.AnimeCategories.ToList()
                : Settings.Categories.ToList();
        }
    }
}
