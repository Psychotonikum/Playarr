using Playarr.Common.Messaging;
using Playarr.Core.Games;

namespace Playarr.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Game Game { get; set; }
        public bool Updated { get; set; }

        public MediaCoversUpdatedEvent(Game game, bool updated)
        {
            Game = game;
            Updated = updated;
        }
    }
}
