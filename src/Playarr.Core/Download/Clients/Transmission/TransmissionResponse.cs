using System.Collections.Generic;

namespace Playarr.Core.Download.Clients.Transmission
{
    public class TransmissionResponse
    {
        public string Result { get; set; }
        public Dictionary<string, object> Arguments { get; set; }
    }
}
