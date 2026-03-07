using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class EpisodeDeleteMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public RomFile RomFile { get; set; }

        public DeleteMediaFileReason Reason { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
