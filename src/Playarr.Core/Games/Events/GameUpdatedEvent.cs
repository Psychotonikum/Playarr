using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesUpdatedEvent : IEvent
    {
        public Game Game { get; private set; }

        public SeriesUpdatedEvent(Game game)
        {
            Game = game;
        }
    }
}
