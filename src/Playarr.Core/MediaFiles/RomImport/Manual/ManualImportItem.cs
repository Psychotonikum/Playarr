using System.Collections.Generic;
using Playarr.Core.CustomFormats;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Manual
{
    public class ManualImportItem
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public Game Game { get; set; }
        public int? SeasonNumber { get; set; }
        public List<Rom> Roms { get; set; }
        public int? EpisodeFileId { get; set; }
        public QualityModel Quality { get; set; } = new();
        public List<Language> Languages { get; set; } = new();
        public string ReleaseGroup { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormat> CustomFormats { get; set; } = new();
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public IEnumerable<ImportRejection> Rejections { get; set; } = new List<ImportRejection>();
    }
}
