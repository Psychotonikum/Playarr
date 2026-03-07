using Playarr.Core.Datastore.Events;

namespace Playarr.SignalR
{
    public class SignalRMessage
    {
        public object Body { get; set; }
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ModelAction Action { get; set; }

        public int? Version { get; set; }
    }
}
