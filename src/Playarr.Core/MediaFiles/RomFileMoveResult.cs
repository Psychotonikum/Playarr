using System.Collections.Generic;

namespace Playarr.Core.MediaFiles
{
    public class RomFileMoveResult
    {
        public RomFileMoveResult()
        {
            OldFiles = new List<DeletedRomFile>();
        }

        public RomFile RomFile { get; set; }
        public List<DeletedRomFile> OldFiles { get; set; }
    }
}
