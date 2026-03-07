namespace Playarr.Core.MediaFiles
{
    public class DeletedRomFile
    {
        public string RecycleBinPath { get; set; }
        public RomFile RomFile { get; set; }

        public DeletedRomFile(RomFile romFile, string recycleBinPath)
        {
            RomFile = romFile;
            RecycleBinPath = recycleBinPath;
        }
    }
}
