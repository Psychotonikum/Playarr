using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class SeriesAddMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
