using Newtonsoft.Json;
using Playarr.Core.Download.Clients.NzbVortex.JsonConverters;

namespace Playarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexResponseBase
    {
        [JsonConverter(typeof(NzbVortexResultTypeConverter))]
        public NzbVortexResultType Result { get; set; }
    }
}
