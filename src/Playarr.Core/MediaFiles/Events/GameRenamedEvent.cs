using System.Collections.Generic;
using Playarr.Common.Messaging;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.Events
{
    public class SeriesRenamedEvent : IEvent
    {
        public Game Game { get; private set; }
        public List<RenamedRomFile> RenamedFiles { get; private set; }

        public SeriesRenamedEvent(Game game, List<RenamedRomFile> renamedFiles)
        {
            Game = game;
            RenamedFiles = renamedFiles;
        }
    }
}
