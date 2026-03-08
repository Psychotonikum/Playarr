using System.Collections.Generic;

namespace Playarr.Core.Games
{
    public class ScannedGameEntry
    {
        public ScannedGameEntry()
        {
            Updates = new List<ScannedRomFile>();
            Dlcs = new List<ScannedRomFile>();
        }

        public string GameTitle { get; set; }
        public int GameSystemId { get; set; }
        public string SystemFolderName { get; set; }
        public GameSystemType SystemType { get; set; }

        public ScannedRomFile BaseFile { get; set; }
        public List<ScannedRomFile> Updates { get; set; }
        public List<ScannedRomFile> Dlcs { get; set; }

        public bool HasBase => BaseFile != null;
        public bool IsMissingBase => !HasBase && (Updates.Count > 0 || Dlcs.Count > 0);
    }

    public class ScannedRomFile
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public RomFileType FileType { get; set; }
        public string Version { get; set; }
        public string DlcIndex { get; set; }
        public string Region { get; set; }
        public long Size { get; set; }
    }
}
