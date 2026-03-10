using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Http;
using Playarr.Core.IndexerSearch.Definitions;

namespace Playarr.Core.Indexers.Nyaa
{
    public class NyaaRequestGenerator : IIndexerRequestGenerator
    {
        public NyaaSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (Settings.AnimeStandardFormatSearch && searchCriteria.PlatformNumber > 0 && searchCriteria.EpisodeNumber > 0)
            {
                foreach (var searchTitle in searchCriteria.SceneTitles.Select(PrepareQuery))
                {
                    pageableRequests.Add(GetPagedRequests($"{searchTitle}+s{searchCriteria.PlatformNumber:00}e{searchCriteria.EpisodeNumber:00}"));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (Settings.AnimeStandardFormatSearch && searchCriteria.PlatformNumber > 0)
            {
                foreach (var searchTitle in searchCriteria.SceneTitles.Select(PrepareQuery))
                {
                    pageableRequests.Add(GetPagedRequests($"{searchTitle}+s{searchCriteria.PlatformNumber:00}"));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(PrepareQuery(queryTitle)));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string term)
        {
            var baseUrl = $"{Settings.BaseUrl.TrimEnd('/')}/?page=rss{Settings.AdditionalParameters}";

            if (term != null)
            {
                baseUrl += "&term=" + term;
            }

            yield return new IndexerRequest(baseUrl, HttpAccept.Rss);
        }

        private string PrepareQuery(string query)
        {
            return query.Replace(' ', '+');
        }
    }
}
