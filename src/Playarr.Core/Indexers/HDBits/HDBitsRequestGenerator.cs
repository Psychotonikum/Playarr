using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Playarr.Common.Http;
using Playarr.Common.Serializer;
using Playarr.Core.IndexerSearch.Definitions;

namespace Playarr.Core.Indexers.HDBits
{
    public class HDBitsRequestGenerator : IIndexerRequestGenerator
    {
        public HDBitsSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(new TorrentQuery()));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var queryBase = new TorrentQuery();

            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var rom in searchCriteria.Roms)
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Platform = rom.SeasonNumber;
                    query.TvdbInfo.Rom = rom.EpisodeNumber;
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeSeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var queryBase = new TorrentQuery();

            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var platformNumber in searchCriteria.Roms.Select(e => e.SeasonNumber).Distinct())
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Platform = platformNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var query = new TorrentQuery();

            if (TryAddSearchParameters(query, searchCriteria))
            {
                query.Search = searchCriteria.AirDate.ToString("yyyy-MM-dd");

                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var query = new TorrentQuery();

            if (TryAddSearchParameters(query, searchCriteria))
            {
                query.Search = $"{searchCriteria.Year}-";

                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var queryBase = new TorrentQuery();

            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var platformNumber in searchCriteria.Roms.Select(e => e.SeasonNumber).Distinct())
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Platform = platformNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var queryBase = new TorrentQuery();

            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var rom in searchCriteria.Roms)
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Platform = rom.SeasonNumber;
                    query.TvdbInfo.Rom = rom.EpisodeNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        private bool TryAddSearchParameters(TorrentQuery query, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Game.TvdbId != 0)
            {
                query.TvdbInfo ??= new TvdbInfo();
                query.TvdbInfo.Id = searchCriteria.Game.TvdbId;

                return true;
            }

            return false;
        }

        private IEnumerable<IndexerRequest> GetRequest(TorrentQuery query)
        {
            var request = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/api/torrents")
                .Build();

            request.Method = HttpMethod.Post;
            const string appJson = "application/json";
            request.Headers.Accept = appJson;
            request.Headers.ContentType = appJson;

            query.Username = Settings.Username;
            query.Passkey = Settings.ApiKey;

            query.Category = Settings.Categories.ToArray();
            query.Codec = Settings.Codecs.ToArray();
            query.Medium = Settings.Mediums.ToArray();

            query.Limit = 100;

            request.SetContent(query.ToJson());
            request.ContentSummary = query.ToJson(Formatting.None);

            yield return new IndexerRequest(request);
        }
    }
}
