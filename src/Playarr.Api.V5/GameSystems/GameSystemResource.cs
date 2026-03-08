using Playarr.Core.Games;
using Playarr.Http.REST;

namespace Playarr.Api.V5.GameSystems;

public class GameSystemResource : RestResource
{
    public string? Name { get; set; }
    public string? FolderName { get; set; }
    public int SystemType { get; set; }
    public List<string>? FileExtensions { get; set; }
    public string? NamingFormat { get; set; }
    public string? UpdateNamingFormat { get; set; }
    public string? DlcNamingFormat { get; set; }
    public string? BaseFolderName { get; set; }
    public string? UpdateFolderName { get; set; }
    public string? DlcFolderName { get; set; }
    public HashSet<int>? Tags { get; set; }
}

public static class GameSystemResourceMapper
{
    public static GameSystemResource ToResource(this GameSystem model)
    {
        return new GameSystemResource
        {
            Id = model.Id,
            Name = model.Name,
            FolderName = model.FolderName,
            SystemType = (int)model.SystemType,
            FileExtensions = model.FileExtensions,
            NamingFormat = model.NamingFormat,
            UpdateNamingFormat = model.UpdateNamingFormat,
            DlcNamingFormat = model.DlcNamingFormat,
            BaseFolderName = model.BaseFolderName,
            UpdateFolderName = model.UpdateFolderName,
            DlcFolderName = model.DlcFolderName,
            Tags = model.Tags
        };
    }

    public static GameSystem ToModel(this GameSystemResource resource)
    {
        return new GameSystem
        {
            Id = resource.Id,
            Name = resource.Name,
            FolderName = resource.FolderName,
            SystemType = (GameSystemType)resource.SystemType,
            FileExtensions = resource.FileExtensions ?? new List<string>(),
            NamingFormat = resource.NamingFormat,
            UpdateNamingFormat = resource.UpdateNamingFormat,
            DlcNamingFormat = resource.DlcNamingFormat,
            BaseFolderName = resource.BaseFolderName,
            UpdateFolderName = resource.UpdateFolderName,
            DlcFolderName = resource.DlcFolderName,
            Tags = resource.Tags ?? new HashSet<int>()
        };
    }

    public static List<GameSystemResource> ToResource(this IEnumerable<GameSystem> models)
    {
        return models.Select(ToResource).ToList();
    }
}
