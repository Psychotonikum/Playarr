using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Http;
using Playarr.Core.IndexerSearch.Definitions;

namespace Playarr.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetRequestGenerator : IIndexerRequestGenerator
    {
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public BroadcastheNetSettings Settings { get; set; }

        public int? LastRecentTorrentId { get; set; }

        public BroadcastheNetRequestGenerator()
        {
            MaxPages = 10;
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (LastRecentTorrentId is > 0)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, new BroadcastheNetTorrentQuery
                {
                    Id = ">=" + (LastRecentTorrentId.Value - 100)
                }));
            }

            pageableRequests.AddTier(GetPagedRequests(MaxPages, new BroadcastheNetTorrentQuery
            {
                Age = "<=86400"
            }));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var parameters = new BroadcastheNetTorrentQuery();

            if (AddGameSearchParameters(parameters, searchCriteria))
            {
                foreach (var rom in searchCriteria.Roms)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Rom";
                    parameters.Name = $"S{rom.PlatformNumber:00}%E{rom.EpisodeNumber:00}%";

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var parameters = new BroadcastheNetTorrentQuery();

            if (AddGameSearchParameters(parameters, searchCriteria))
            {
                foreach (var platformNumber in searchCriteria.Roms.Select(v => v.PlatformNumber).Distinct())
                {
                    parameters.Category = "Platform";
                    parameters.Name = $"Platform {platformNumber}%";

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));

                    parameters = parameters.Clone();

                    parameters.Category = "Rom";
                    parameters.Name = $"S{platformNumber:00}E%";

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var parameters = new BroadcastheNetTorrentQuery();

            if (AddGameSearchParameters(parameters, searchCriteria))
            {
                var episodeQueryTitle = searchCriteria.Roms.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                    .Select(e => SearchCriteriaBase.GetCleanSceneTitle(e.Title))
                    .ToArray();

                foreach (var queryTitle in episodeQueryTitle)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Rom";
                    parameters.Name = $"%{queryTitle}%";

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        private bool AddGameSearchParameters(BroadcastheNetTorrentQuery parameters, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Game.IgdbId != 0)
            {
                parameters.Igdb = $"{searchCriteria.Game.IgdbId}";
                return true;
            }

            if (searchCriteria.Game.MobyGamesId != 0)
            {
                parameters.Tvrage = $"{searchCriteria.Game.MobyGamesId}";
                return true;
            }

            // BTN is very neatly managed, so it's unlikely they map tvrage/igdb wrongly.
            return false;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, BroadcastheNetTorrentQuery parameters)
        {
            var builder = new JsonRpcRequestBuilder(Settings.BaseUrl)
                .Call("getTorrents", Settings.ApiKey, parameters, PageSize, 0);
            builder.SuppressHttpError = true;

            for (var page = 0; page < maxPages; page++)
            {
                builder.JsonParameters[3] = page * PageSize;

                yield return new IndexerRequest(builder.Build());
            }
        }
    }
}
