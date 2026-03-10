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
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch", ""));
            }
            else if (capabilities.SupportedSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", ""));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsTvTextSearches && !SupportsTvIdSearches)
            {
                _logger.Debug("Indexer capabilities lacking q, title, igdbid, imdbid, rid and tvmazeid parameters, no Standard game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            var categories = GetSearchCategories(searchCriteria);

            // Games don't use season/episode params - search by ID and title only
            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    "");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    "");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    "");
            }

            // Fallback: generic search for ROM-style releases
            pageableRequests.AddTier();
            AddGenericSearchRequests(pageableRequests, categories, searchCriteria);

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!SupportsTvTextSearches && !SupportsTvIdSearches)
            {
                _logger.Debug("Indexer capabilities lacking q, title, igdbid, imdbid, rid and tvmazeid parameters, no Standard game search possible: {0}", Definition.Name);

                return pageableRequests;
            }

            var categories = GetSearchCategories(searchCriteria);

            // Games don't use season params - search by ID and title only
            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    "");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    "");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    categories,
                    searchCriteria,
                    "");
            }

            // Fallback: generic search for ROM-style releases
            pageableRequests.AddTier();
            AddGenericSearchRequests(pageableRequests, categories, searchCriteria);

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

            // Always append a title query alongside ID parameters so indexers that
            // advertise but don't actually support game-specific IDs (e.g. igdbid)
            // still return title-filtered results instead of a generic category browse.
            var titleQuery = "";
            if (SupportsTvQuerySearch)
            {
                var queryTitles = TvTextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;
                var firstTitle = queryTitles.FirstOrDefault();
                if (firstTitle.IsNotNullOrWhiteSpace())
                {
                    titleQuery = "&q=" + NewsnabifyTitle(firstTitle);
                }
            }

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

                chain.Add(GetPagedRequests(MaxPages, categories, "tvsearch", ids + titleQuery + parameters));
            }
            else
            {
                if (includeIgdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&igdbid={searchCriteria.Game.IgdbId}{titleQuery}{parameters}"));
                }
                else if (includeImdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&imdbid={searchCriteria.Game.ImdbId}{titleQuery}{parameters}"));
                }
                else if (includeTvRageSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&rid={searchCriteria.Game.MobyGamesId}{titleQuery}{parameters}"));
                }
                else if (includeTvMazeSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&tvmazeid={searchCriteria.Game.RawgId}{titleQuery}{parameters}"));
                }
                else if (includeTmdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        $"&tmdbid={searchCriteria.Game.TmdbId}{titleQuery}{parameters}"));
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
                        categories,
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
                        categories,
                        "tvsearch",
                        $"&q={NewsnabifyTitle(queryTitle)}{parameters}"));
                }
            }
        }

        private void AddGenericSearchRequests(IndexerPageableRequestChain chain, IEnumerable<int> categories, SearchCriteriaBase searchCriteria)
        {
            var queryTitles = TvTextSearchEngine == "raw" ? searchCriteria.AllSceneTitles : searchCriteria.CleanSceneTitles;
            foreach (var queryTitle in queryTitles)
            {
                chain.Add(GetPagedRequests(MaxPages,
                    categories,
                    "search",
                    $"&q={NewsnabifyTitle(queryTitle)}"));
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

        // Maps platform name keywords to Newznab Console subcategory IDs.
        // Platforms without a specific match fall back to the configured categories (default: 1000 Console).
        private static readonly Dictionary<string, int> PlatformCategoryMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // Nintendo
            { "switch", 1130 },
            { "nintendo switch", 1130 },
            { "3ds", 1090 },
            { "nintendo 3ds", 1090 },
            { "new nintendo 3ds", 1090 },
            { "ds", 1010 },
            { "nds", 1010 },
            { "nintendo ds", 1010 },
            { "nintendo dsi", 1010 },
            { "wii u", 1060 },
            { "wiiu", 1060 },
            { "wii", 1030 },
            { "nintendo wii", 1030 },

            // PlayStation
            { "ps5", 1140 },
            { "playstation 5", 1140 },
            { "ps4", 1120 },
            { "playstation 4", 1120 },
            { "ps3", 1070 },
            { "playstation 3", 1070 },
            { "ps vita", 1100 },
            { "playstation vita", 1100 },
            { "psvita", 1100 },
            { "psp", 1020 },
            { "playstation portable", 1020 },

            // Xbox
            { "xbox series x", 1150 },
            { "xbox series s", 1150 },
            { "xbox series x|s", 1150 },
            { "xbox one", 1110 },
            { "xboxone", 1110 },
            { "xbox 360", 1050 },
            { "xbox360", 1050 },
            { "xbox", 1040 },

            // PC
            { "pc", 1180 },
            { "windows", 1180 },
            { "linux", 1180 },
            { "mac", 1180 },
            { "macos", 1180 },
        };

        private static int? ResolvePlatformCategory(string platformName)
        {
            if (platformName.IsNullOrWhiteSpace())
            {
                return null;
            }

            // Direct match first
            if (PlatformCategoryMap.TryGetValue(platformName.Trim(), out var category))
            {
                return category;
            }

            // Partial match: check if any key is contained in the platform name
            foreach (var kvp in PlatformCategoryMap)
            {
                if (platformName.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private IList<int> GetSearchCategories(SearchCriteriaBase searchCriteria)
        {
            var configuredCategories = Settings.Categories.ToList();

            // Try to resolve platform-specific category from search criteria
            int? platformNumber = null;

            if (searchCriteria is SingleEpisodeSearchCriteria singleRom)
            {
                platformNumber = singleRom.PlatformNumber;
            }
            else if (searchCriteria is SeasonSearchCriteria platformSearch)
            {
                platformNumber = platformSearch.PlatformNumber;
            }

            if (platformNumber.HasValue && searchCriteria.Game?.Platforms != null)
            {
                var platform = searchCriteria.Game.Platforms.FirstOrDefault(p => p.PlatformNumber == platformNumber.Value);
                var platformCategory = ResolvePlatformCategory(platform?.Title);

                if (platformCategory.HasValue)
                {
                    _logger.Debug("Resolved platform '{0}' to Newznab category {1}", platform.Title, platformCategory.Value);

                    // Use the platform-specific category, excluding other console subcategories that don't match
                    var nonConsoleCategories = configuredCategories.Where(c => c < 1000 || c >= 2000).ToList();
                    nonConsoleCategories.Add(platformCategory.Value);

                    return nonConsoleCategories;
                }
            }

            return configuredCategories;
        }
    }
}
