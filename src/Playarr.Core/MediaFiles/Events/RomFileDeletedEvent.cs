using Playarr.Common.Messaging;

namespace Playarr.Core.MediaFiles.Events
{
    public class RomFileDeletedEvent : IEvent
    {
        public RomFile RomFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public RomFileDeletedEvent(RomFile romFile, DeleteMediaFileReason reason)
        {
            RomFile = romFile;
            Reason = reason;
        }
    }
}
