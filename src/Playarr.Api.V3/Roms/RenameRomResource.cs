using System.Collections.Generic;
using System.Linq;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Roms
{
    public class RenameRomResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public List<int> RomNumbers { get; set; }
        public int EpisodeFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }

    public static class RenameRomResourceMapper
    {
        public static RenameRomResource ToResource(this Playarr.Core.MediaFiles.RenameRomFilePreview model)
        {
            if (model == null)
            {
                return null;
            }

            return new RenameRomResource
            {
                Id = model.EpisodeFileId,
                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                RomNumbers = model.RomNumbers.ToList(),
                EpisodeFileId = model.EpisodeFileId,
                ExistingPath = model.ExistingPath,
                NewPath = model.NewPath
            };
        }

        public static List<RenameRomResource> ToResource(this IEnumerable<Playarr.Core.MediaFiles.RenameRomFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
