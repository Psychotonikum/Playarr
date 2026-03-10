using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Games.ScraperImport;
using Playarr.Http;

namespace Playarr.Api.V3.Game
{
    [V3ApiController("game/scraperimport")]
    public class ScraperImportController : Controller
    {
        private readonly IScraperImportService _scraperImportService;

        public ScraperImportController(IScraperImportService scraperImportService)
        {
            _scraperImportService = scraperImportService;
        }

        [HttpGet]
        public List<ScraperImportItemResource> Scan([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new List<ScraperImportItemResource>();
            }

            var items = _scraperImportService.Scan(path);

            return items.Select(i => new ScraperImportItemResource
            {
                GameName = i.GameName,
                SystemName = i.SystemName,
                SystemFolder = i.SystemFolder,
                SystemType = i.SystemType.ToString(),
                Files = i.Files.Select(f => new ScraperImportFileResource
                {
                    SourcePath = f.SourcePath,
                    FileName = f.FileName,
                    Size = f.Size,
                    FileType = f.FileType
                }).ToList()
            }).ToList();
        }

        [HttpPost]
        public List<ScraperImportResultResource> Import([FromBody] List<ScraperImportRequestResource> resources)
        {
            var requests = resources.Select(r => new ScraperImportRequest
            {
                GameName = r.GameName,
                SystemFolder = r.SystemFolder,
                IgdbId = r.IgdbId,
                QualityProfileId = r.QualityProfileId,
                Files = r.Files.Select(f => new ScraperImportFile
                {
                    SourcePath = f.SourcePath,
                    FileName = f.FileName,
                    Size = f.Size,
                    FileType = f.FileType
                }).ToList()
            }).ToList();

            var results = _scraperImportService.Import(requests);

            return results.Select(r => new ScraperImportResultResource
            {
                GameName = r.GameName,
                GameId = r.GameId,
                Success = r.Success,
                FilesImported = r.FilesImported,
                Error = r.Error
            }).ToList();
        }
    }

    public class ScraperImportItemResource
    {
        public string GameName { get; set; }
        public string SystemName { get; set; }
        public string SystemFolder { get; set; }
        public string SystemType { get; set; }
        public List<ScraperImportFileResource> Files { get; set; }
    }

    public class ScraperImportFileResource
    {
        public string SourcePath { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public string FileType { get; set; }
    }

    public class ScraperImportRequestResource
    {
        public string GameName { get; set; }
        public string SystemFolder { get; set; }
        public int IgdbId { get; set; }
        public int QualityProfileId { get; set; }
        public List<ScraperImportFileResource> Files { get; set; }
    }

    public class ScraperImportResultResource
    {
        public string GameName { get; set; }
        public int GameId { get; set; }
        public bool Success { get; set; }
        public int FilesImported { get; set; }
        public string Error { get; set; }
    }
}
