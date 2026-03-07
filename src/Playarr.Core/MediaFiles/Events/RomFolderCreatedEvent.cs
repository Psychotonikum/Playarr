using Playarr.Common.Messaging;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.Events
{
    public class EpisodeFolderCreatedEvent : IEvent
    {
        public Game Game { get; private set; }
        public RomFile RomFile { get; private set; }
        public string GameFolder { get; set; }
        public string PlatformFolder { get; set; }
        public string EpisodeFolder { get; set; }

        public EpisodeFolderCreatedEvent(Game game, RomFile romFile)
        {
            Game = game;
            RomFile = romFile;
        }
    }
}
