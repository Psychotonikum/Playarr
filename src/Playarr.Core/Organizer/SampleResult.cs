using System.Collections.Generic;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Game Game { get; set; }
        public List<Rom> Roms { get; set; }
        public RomFile RomFile { get; set; }
    }
}
