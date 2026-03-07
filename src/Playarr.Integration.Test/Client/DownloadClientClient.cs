using System.Collections.Generic;
using RestSharp;
using Playarr.Api.V3.DownloadClient;

namespace Playarr.Integration.Test.Client
{
    public class DownloadClientClient : ClientBase<DownloadClientResource>
    {
        public DownloadClientClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<DownloadClientResource> Schema()
        {
            var request = BuildRequest("/schema");
            return Get<List<DownloadClientResource>>(request);
        }
    }
}
