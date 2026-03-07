using RestSharp;
using Playarr.Api.V3.Indexers;

namespace Playarr.Integration.Test.Client
{
    public class ReleasePushClient : ClientBase<ReleaseResource>
    {
        public ReleasePushClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "release/push")
        {
        }
    }
}
