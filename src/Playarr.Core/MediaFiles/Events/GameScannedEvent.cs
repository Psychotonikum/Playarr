using System.Collections.Generic;
using Playarr.Common.Messaging;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.Events
{
    public class SeriesScannedEvent : IEvent
    {
        public Game Game { get; private set; }
        public List<string> PossibleExtraFiles { get; set; }

        public SeriesScannedEvent(Game game, List<string> possibleExtraFiles)
        {
            Game = game;
            PossibleExtraFiles = possibleExtraFiles;
        }
    }
}
