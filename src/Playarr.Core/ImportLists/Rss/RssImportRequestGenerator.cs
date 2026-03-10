using System.Collections.Generic;
using Playarr.Common.Http;

namespace Playarr.Core.ImportLists.Rss
{
    public class RssImportRequestGenerator<TSettings> : IImportListRequestGenerator
        where TSettings : RssImportBaseSettings<TSettings>, new()
    {
        public RssImportBaseSettings<TSettings> Settings { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetGameRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetGameRequest()
        {
            yield return new ImportListRequest(Settings.Url, HttpAccept.Rss);
        }
    }
}
