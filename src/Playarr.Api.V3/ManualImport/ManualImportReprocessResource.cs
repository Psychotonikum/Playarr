using System.Collections.Generic;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Api.V3.CustomFormats;
using Playarr.Api.V3.Roms;
using Playarr.Http.REST;

namespace Playarr.Api.V3.ManualImport
{
    public class ManualImportReprocessResource : RestResource
    {
        public string Path { get; set; }
        public int GameId { get; set; }
        public int? PlatformNumber { get; set; }
        public List<RomResource> Roms { get; set; }
        public List<int> RomIds { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public IEnumerable<ImportRejectionResource> Rejections { get; set; }
    }
}
