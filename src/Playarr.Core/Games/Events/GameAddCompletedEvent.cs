using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesAddCompletedEvent : IEvent
    {
        public Game Game { get; private set; }

        public SeriesAddCompletedEvent(Game game)
        {
            Game = game;
        }
    }
}
