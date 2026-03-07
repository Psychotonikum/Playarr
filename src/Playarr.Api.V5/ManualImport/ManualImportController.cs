using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.EpisodeImport.Manual;
using Playarr.Core.Qualities;
using Playarr.Http;
using Playarr.Http.REST;

namespace Playarr.Api.V5.ManualImport;

[V5ApiController]
public class ManualImportController : Controller
{
    private readonly IManualImportService _manualImportService;

    public ManualImportController(IManualImportService manualImportService)
    {
        _manualImportService = manualImportService;
    }

    [HttpGet]
    [Produces("application/json")]
    public List<ManualImportResource> GetMediaFiles(string? folder, [FromQuery] string[]? downloadIds, int? gameId, int? platformNumber, bool filterExistingFiles = true)
    {
        if (gameId.HasValue && downloadIds == null)
        {
            return _manualImportService.GetMediaFiles(gameId.Value, platformNumber)
                .ToResource()
                .Select(AddQualityWeight)
                .ToList();
        }

        if (downloadIds != null && downloadIds.Any())
        {
            var files = new List<ManualImportItem>();

            foreach (var downloadId in downloadIds.Distinct())
            {
                files.AddRange(_manualImportService.GetMediaFiles(null, downloadId, gameId, filterExistingFiles));
            }

            return files.ToResource()
                .Select(AddQualityWeight)
                .ToList();
        }

        return _manualImportService.GetMediaFiles(folder, null, gameId, filterExistingFiles)
            .ToResource()
            .Select(AddQualityWeight)
            .ToList();
    }

    [HttpPost]
    [Consumes("application/json")]
    public List<ManualImportResource> ReprocessItems([FromBody] List<ManualImportReprocessResource> items)
    {
        if (items is { Count: 0 })
        {
            throw new BadRequestException("items must be provided");
        }

        var updatedItems = new List<ManualImportItem>();

        foreach (var item in items)
        {
            var processedItem = _manualImportService.ReprocessItem(item.Path, item.DownloadId, item.SeriesId, item.SeasonNumber, item.RomIds ?? new List<int>(), item.ReleaseGroup, item.Quality, item.Languages, item.IndexerFlags, item.ReleaseType);

            // Only use the processed item's languages, quality, and release group if the user hasn't specified them.
            // Languages won't be returned when reprocessing if the platform/rom isn't filled in yet and we don't want to return no languages to the client.
            if (processedItem.Languages.Empty() || item.Languages.Count > 1 || (item.Languages.SingleOrDefault() ?? Language.Unknown) == Language.Unknown)
            {
                processedItem.Languages = item.Languages;
            }

            if (item.Quality?.Quality != Quality.Unknown)
            {
                processedItem.Quality = item.Quality;
            }

            if (item.ReleaseGroup.IsNotNullOrWhiteSpace())
            {
                processedItem.ReleaseGroup = item.ReleaseGroup;
            }

            if (item.SeasonNumber.HasValue && !processedItem.SeasonNumber.HasValue)
            {
                processedItem.SeasonNumber = item.SeasonNumber;
            }

            updatedItems.Add(processedItem);
        }

        return updatedItems.ToResource();
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
