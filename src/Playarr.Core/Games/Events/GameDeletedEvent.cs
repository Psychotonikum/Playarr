using System.Collections.Generic;
using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesDeletedEvent : IEvent
    {
        public List<Game> Game { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public SeriesDeletedEvent(List<Game> game, bool deleteFiles, bool addImportListExclusion)
        {
            Game = game;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
