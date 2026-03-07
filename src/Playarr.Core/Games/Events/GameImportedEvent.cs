using System.Collections.Generic;
using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class GameImportedEvent : IEvent
    {
        public List<int> GameIds { get; private set; }

        public GameImportedEvent(List<int> gameIds)
        {
            GameIds = gameIds;
        }
    }
}
