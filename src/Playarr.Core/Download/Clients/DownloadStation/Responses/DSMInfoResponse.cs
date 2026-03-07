using Newtonsoft.Json;

namespace Playarr.Core.Download.Clients.DownloadStation.Responses
{
    public class DSMInfoResponse
    {
        [JsonProperty("serial")]
        public string SerialNumber { get; set; }
    }
}
