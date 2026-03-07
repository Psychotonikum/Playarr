using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class GrabMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public RemoteEpisode Rom { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
