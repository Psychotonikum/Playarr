using System.Collections.Generic;
using Playarr.Common.Extensions;
using Playarr.Common.Http;

namespace Playarr.Core.ImportLists.Simkl.User
{
    public class SimklUserRequestGenerator : IImportListRequestGenerator
    {
        public SimklUserSettings Settings { get; set; }

        public string ClientId { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetGameRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetGameRequest()
        {
            var link = $"{Settings.BaseUrl.Trim()}/sync/all-items/{((SimklUserShowType)Settings.ShowType).ToString().ToLowerInvariant()}/{((SimklUserListType)Settings.ListType).ToString().ToLowerInvariant()}";

            var request = new ImportListRequest(link, HttpAccept.Json);

            request.HttpRequest.Headers.Add("simkl-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
