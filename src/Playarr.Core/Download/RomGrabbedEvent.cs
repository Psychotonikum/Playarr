using Playarr.Common.Messaging;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download
{
    public class EpisodeGrabbedEvent : IEvent
    {
        public RemoteRom Rom { get; private set; }
        public int DownloadClientId { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public EpisodeGrabbedEvent(RemoteRom rom)
        {
            Rom = rom;
        }
    }
}
