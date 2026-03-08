using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.EpisodeImport.Manual;
using Playarr.Core.Qualities;
using Playarr.Api.V3.CustomFormats;
using Playarr.Api.V3.Roms;
using Playarr.Http;
using Playarr.Http.REST;

namespace Playarr.Api.V3.ManualImport
{
    [V3ApiController]
    public class ManualImportController : Controller
    {
        private readonly IManualImportService _manualImportService;

        public ManualImportController(IManualImportService manualImportService)
        {
            _manualImportService = manualImportService;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<ManualImportResource> GetMediaFiles(string folder, string downloadId, int? gameId, int? platformNumber, bool filterExistingFiles = true)
        {
            if (gameId.HasValue && downloadId.IsNullOrWhiteSpace())
            {
                return _manualImportService.GetMediaFiles(gameId.Value, platformNumber).ToResource().Select(AddQualityWeight).ToList();
            }

            return _manualImportService.GetMediaFiles(folder, downloadId, gameId, filterExistingFiles).ToResource().Select(AddQualityWeight).ToList();
        }

        [HttpPost]
        [Consumes("application/json")]
        public object ReprocessItems([FromBody] List<ManualImportReprocessResource> items)
        {
            if (items is { Count: 0 })
            {
                throw new BadRequestException("items must be provided");
            }

            foreach (var item in items)
            {
                var processedItem = _manualImportService.ReprocessItem(item.Path, item.DownloadId, item.GameId, item.PlatformNumber, item.RomIds ?? new List<int>(), item.ReleaseGroup, item.Quality, item.Languages, item.IndexerFlags, item.ReleaseType);

                item.PlatformNumber = processedItem.PlatformNumber;
                item.Roms = processedItem.Roms.ToResource();
                item.ReleaseType = processedItem.ReleaseType;
                item.IndexerFlags = processedItem.IndexerFlags;
                item.Rejections = processedItem.Rejections.Select(r => r.ToResource());
                item.CustomFormats = processedItem.CustomFormats.ToResource(false);
                item.CustomFormatScore = processedItem.CustomFormatScore;

                // Only set the language/quality if they're unknown and languages were returned.
                // Languages won't be returned when reprocessing if the platform/rom isn't filled in yet and we don't want to return no languages to the client.
                if (item.Languages.Count <= 1 && (item.Languages.SingleOrDefault() ?? Language.Unknown) == Language.Unknown && processedItem.Languages.Any())
                {
                    item.Languages = processedItem.Languages;
                }

                if (item.Quality?.Quality == Quality.Unknown)
                {
                    item.Quality = processedItem.Quality;
                }

                if (item.ReleaseGroup.IsNullOrWhiteSpace())
                {
                    item.ReleaseGroup = processedItem.ReleaseGroup;
                }

                // Clear rom IDs in favour of the full rom
                item.RomIds = null;
            }

            return items;
        }

        private ManualImportResource AddQualityWeight(ManualImportResource item)
        {
            if (item.Quality != null)
            {
                item.QualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == item.Quality.Quality).Weight;
                item.QualityWeight += item.Quality.Revision.Real * 10;
                item.QualityWeight += item.Quality.Revision.Version;
            }

            return item;
        }
    }
}
