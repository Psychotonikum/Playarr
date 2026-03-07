using Playarr.Common.Messaging;

namespace Playarr.Core.MediaFiles.Events
{
    public class RomFileAddedEvent : IEvent
    {
        public RomFile RomFile { get; private set; }

        public RomFileAddedEvent(RomFile romFile)
        {
            RomFile = romFile;
        }
    }
}
