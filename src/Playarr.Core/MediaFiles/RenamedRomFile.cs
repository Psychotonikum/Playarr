namespace Playarr.Core.MediaFiles
{
    public class RenamedRomFile
    {
        public RomFile RomFile { get; set; }
        public string PreviousPath { get; set; }
        public string PreviousRelativePath { get; set; }
    }
}
