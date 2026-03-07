using System.Collections.Generic;
using Newtonsoft.Json;

namespace Playarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexQueueResponse : NzbVortexResponseBase
    {
        [JsonProperty(PropertyName = "nzbs")]
        public List<NzbVortexQueueItem> Items { get; set; }
    }
}
