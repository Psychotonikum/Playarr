using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesAddedEvent : IEvent
    {
        public Game Game { get; private set; }

        public SeriesAddedEvent(Game game)
        {
            Game = game;
        }
    }
}
