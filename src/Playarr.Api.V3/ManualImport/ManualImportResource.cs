using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Crypto;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.EpisodeImport.Manual;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Api.V3.CustomFormats;
using Playarr.Api.V3.Roms;
using Playarr.Api.V3.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V3.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public GameResource Game { get; set; }
        public int? PlatformNumber { get; set; }
        public List<RomResource> Roms { get; set; }
        public int? RomFileId { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public IEnumerable<ImportRejectionResource> Rejections { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this ManualImportItem model)
        {
            if (model == null)
            {
                return null;
            }

            var customFormats = model.CustomFormats;
            var customFormatScore = model.Game?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new ManualImportResource
            {
                Id = HashConverter.GetHashInt31(model.Path),
                Path = model.Path,
                RelativePath = model.RelativePath,
                FolderName = model.FolderName,
                Name = model.Name,
                Size = model.Size,
                Game = model.Game.ToResource(),
                PlatformNumber = model.PlatformNumber,
                Roms = model.Roms.ToResource(),
                RomFileId = model.EpisodeFileId,
                ReleaseGroup = model.ReleaseGroup,
                Quality = model.Quality,
                Languages = model.Languages,
                CustomFormats = customFormats.ToResource(false),
                CustomFormatScore = customFormatScore,

                // QualityWeight
                DownloadId = model.DownloadId,
                IndexerFlags = model.IndexerFlags,
                ReleaseType = model.ReleaseType,
                Rejections = model.Rejections.Select(r => r.ToResource())
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }

    public class ImportRejectionResource
    {
        public string Reason { get; set; }
        public RejectionType Type { get; set; }
    }

    public static class ImportRejectionResourceMapper
    {
        public static ImportRejectionResource ToResource(this ImportRejection rejection)
        {
            if (rejection == null)
            {
                return null;
            }

            return new ImportRejectionResource
            {
                Reason = rejection.Message,
                Type = rejection.Type
            };
        }
    }
}
