using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesMovedEvent : IEvent
    {
        public Game Game { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public SeriesMovedEvent(Game game, string sourcePath, string destinationPath)
        {
            Game = game;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
