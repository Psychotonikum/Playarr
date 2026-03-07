using Playarr.Common.Messaging;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download
{
    public class EpisodeGrabbedEvent : IEvent
    {
        public RemoteEpisode Rom { get; private set; }
        public int DownloadClientId { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public EpisodeGrabbedEvent(RemoteEpisode rom)
        {
            Rom = rom;
        }
    }
}
