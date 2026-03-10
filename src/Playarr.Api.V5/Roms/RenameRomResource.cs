using Playarr.Http.REST;

namespace Playarr.Api.V5.Roms;

public class RenameRomResource : RestResource
{
    public int GameId { get; set; }
    public int PlatformNumber { get; set; }
    public List<int> RomNumbers { get; set; } = [];
    public int RomFileId { get; set; }
    public string? ExistingPath { get; set; }
    public string? NewPath { get; set; }
}

public static class RenameRomResourceMapper
{
    public static RenameRomResource ToResource(this Playarr.Core.MediaFiles.RenameRomFilePreview model)
    {
        return new RenameRomResource
        {
            Id = model.EpisodeFileId,
            GameId = model.GameId,
            PlatformNumber = model.PlatformNumber,
            RomNumbers = model.RomNumbers.ToList(),
            RomFileId = model.EpisodeFileId,
            ExistingPath = model.ExistingPath,
            NewPath = model.NewPath
        };
    }

    public static List<RenameRomResource> ToResource(this IEnumerable<Playarr.Core.MediaFiles.RenameRomFilePreview> models)
    {
        return models.Select(ToResource).ToList();
    }
}
