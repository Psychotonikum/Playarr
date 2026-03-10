using System.Collections.Generic;

namespace Playarr.Core.Games.ScraperImport
{
    public class ScraperImportItem
    {
        public string GameName { get; set; }
        public string SystemName { get; set; }
        public string SystemFolder { get; set; }
        public GameSystemType SystemType { get; set; }
        public List<ScraperImportFile> Files { get; set; }
    }

    public class ScraperImportFile
    {
        public string SourcePath { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public string FileType { get; set; }
    }

    public class ScraperImportRequest
    {
        public string GameName { get; set; }
        public string SystemFolder { get; set; }
        public int IgdbId { get; set; }
        public int QualityProfileId { get; set; }
        public List<ScraperImportFile> Files { get; set; }
    }

    public class ScraperImportResult
    {
        public string GameName { get; set; }
        public int GameId { get; set; }
        public bool Success { get; set; }
        public int FilesImported { get; set; }
        public string Error { get; set; }
    }
}
