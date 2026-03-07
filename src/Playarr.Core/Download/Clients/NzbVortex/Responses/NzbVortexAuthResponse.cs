using Newtonsoft.Json;
using Playarr.Core.Download.Clients.NzbVortex.JsonConverters;

namespace Playarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexAuthResponse : NzbVortexResponseBase
    {
        [JsonConverter(typeof(NzbVortexLoginResultTypeConverter))]
        public NzbVortexLoginResultType LoginResult { get; set; }

        public string SessionId { get; set; }
    }
}
