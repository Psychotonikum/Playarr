using System.Collections.Generic;
using System.Collections.ObjectModel;
using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class RomInfoRefreshedEvent : IEvent
    {
        public Game Game { get; set; }
        public ReadOnlyCollection<Rom> Added { get; private set; }
        public ReadOnlyCollection<Rom> Updated { get; private set; }
        public ReadOnlyCollection<Rom> Removed { get; private set; }

        public RomInfoRefreshedEvent(Game game, IList<Rom> added, IList<Rom> updated, IList<Rom> removed)
        {
            Game = game;
            Added = new ReadOnlyCollection<Rom>(added);
            Updated = new ReadOnlyCollection<Rom>(updated);
            Removed = new ReadOnlyCollection<Rom>(removed);
        }
    }
}
