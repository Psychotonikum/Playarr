using System.Collections.Generic;

namespace Playarr.Core.MediaFiles
{
    public class RenameRomFilePreview
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public List<int> RomNumbers { get; set; }
        public int EpisodeFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }
}
