using System.Collections.Generic;
using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesBulkEditedEvent : IEvent
    {
        public List<Game> Game { get; private set; }

        public SeriesBulkEditedEvent(List<Game> game)
        {
            Game = game;
        }
    }
}
