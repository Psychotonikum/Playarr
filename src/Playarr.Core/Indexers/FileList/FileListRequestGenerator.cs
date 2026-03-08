using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.IndexerSearch.Definitions;

namespace Playarr.Core.Indexers.FileList
{
    public class FileListRequestGenerator : IIndexerRequestGenerator
    {
        public FileListSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("latest-torrents", Settings.Categories.Concat(Settings.AnimeCategories), ""));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}&rom={searchCriteria.EpisodeNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}&rom={searchCriteria.EpisodeNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}&rom={searchCriteria.EpisodeNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&platform={searchCriteria.PlatformNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            // FileList has absolute releases in E01 format but also release sin S01E01 format, likely by imdb numbering but we only have igdb numbering... so we try those as fallback to abs.
            AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&platform=0&rom={searchCriteria.AbsoluteEpisodeNumber}");
            pageableRequests.AddTier();
            foreach (var eps in searchCriteria.Roms)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&platform={eps.PlatformNumber}&rom={eps.EpisodeNumber}");
            }

            pageableRequests.AddTier();

            AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&platform=0&rom={searchCriteria.AbsoluteEpisodeNumber}");
            pageableRequests.AddTier();
            foreach (var eps in searchCriteria.Roms)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&platform={eps.PlatformNumber}&rom={eps.EpisodeNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(AnimeSeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&platform={searchCriteria.PlatformNumber}");
            pageableRequests.AddTier();
            AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&platform={searchCriteria.PlatformNumber}");

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private void AddImdbRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            if (searchCriteria.Game.ImdbId.IsNotNullOrWhiteSpace())
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=imdb&query={0}{1}", searchCriteria.Game.ImdbId, parameters)));
            }
        }

        private void AddNameRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            foreach (var sceneTitle in searchCriteria.SceneTitles)
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=name&query={0}{1}", Uri.EscapeDataString(sceneTitle.Trim()), parameters)));
            }
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, IEnumerable<int> categories, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}/api.php?action={1}&category={2}{3}", Settings.BaseUrl.TrimEnd('/'), searchType, categoriesQuery, parameters);

            var request = new IndexerRequest(baseUrl, HttpAccept.Json);
            request.HttpRequest.Credentials = new BasicNetworkCredential(Settings.Username.Trim(), Settings.Passkey.Trim());

            yield return request;
        }
    }
}
