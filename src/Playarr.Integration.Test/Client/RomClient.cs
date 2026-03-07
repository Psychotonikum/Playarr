using System.Collections.Generic;
using RestSharp;
using Playarr.Api.V3.Roms;

namespace Playarr.Integration.Test.Client
{
    public class EpisodeClient : ClientBase<RomResource>
    {
        public EpisodeClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "rom")
        {
        }

        public List<RomResource> GetEpisodesInSeries(int gameId)
        {
            var request = BuildRequest("?gameId=" + gameId.ToString());
            return Get<List<RomResource>>(request);
        }

        public RomResource SetMonitored(RomResource rom)
        {
            var request = BuildRequest(rom.Id.ToString());
            request.AddJsonBody(rom);
            return Put<RomResource>(request);
        }
    }
}
