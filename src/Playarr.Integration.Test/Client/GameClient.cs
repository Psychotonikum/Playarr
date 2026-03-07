using System.Collections.Generic;
using System.Net;
using RestSharp;
using Playarr.Api.V3.Game;

namespace Playarr.Integration.Test.Client
{
    public class SeriesClient : ClientBase<GameResource>
    {
        public SeriesClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<GameResource> Lookup(string term)
        {
            var request = BuildRequest("lookup");
            request.AddQueryParameter("term", term);
            return Get<List<GameResource>>(request);
        }

        public List<GameResource> Editor(GameEditorResource game)
        {
            var request = BuildRequest("editor");
            request.AddJsonBody(game);
            return Put<List<GameResource>>(request);
        }

        public GameResource Get(string slug, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(slug);
            return Get<GameResource>(request, statusCode);
        }
    }

    public class SystemInfoClient : ClientBase<GameResource>
    {
        public SystemInfoClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
