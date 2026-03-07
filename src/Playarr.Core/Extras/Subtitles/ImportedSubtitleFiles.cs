using System.Collections.Generic;
using Playarr.Core.Extras.Files;

namespace Playarr.Core.Extras.Subtitles
{
    public class ImportedSubtitleFiles
    {
        public List<string> SourceFiles { get; set; }
        public List<ExtraFile> SubtitleFiles { get; set; }

        public ImportedSubtitleFiles()
        {
            SourceFiles = new List<string>();
            SubtitleFiles = new List<ExtraFile>();
        }
    }
}
