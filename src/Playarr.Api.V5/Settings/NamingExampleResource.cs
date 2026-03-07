using Playarr.Core.Organizer;

namespace Playarr.Api.V5.Settings;

public class NamingExampleResource
{
    public string? SingleEpisodeExample { get; set; }
    public string? MultiEpisodeExample { get; set; }
    public string? DailyEpisodeExample { get; set; }
    public string? AnimeEpisodeExample { get; set; }
    public string? AnimeMultiEpisodeExample { get; set; }
    public string? GameFolderExample { get; set; }
    public string? PlatformFolderExample { get; set; }
    public string? SpecialsFolderExample { get; set; }
}

public static class NamingConfigResourceMapper
{
    public static NamingSettingsResource ToResource(this NamingConfig model)
    {
        return new NamingSettingsResource
        {
            Id = model.Id,

            RenameEpisodes = model.RenameEpisodes,
            ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
            ColonReplacementFormat = (int)model.ColonReplacementFormat,
            CustomColonReplacementFormat = model.CustomColonReplacementFormat,
            MultiEpisodeStyle = (int)model.MultiEpisodeStyle,
            StandardEpisodeFormat = model.StandardEpisodeFormat,
            DailyEpisodeFormat = model.DailyEpisodeFormat,
            AnimeEpisodeFormat = model.AnimeEpisodeFormat,
            GameFolderFormat = model.GameFolderFormat,
            PlatformFolderFormat = model.PlatformFolderFormat,
            SpecialsFolderFormat = model.SpecialsFolderFormat
        };
    }

    public static NamingConfig ToModel(this NamingSettingsResource resource)
    {
        return new NamingConfig
        {
            Id = resource.Id,

            RenameEpisodes = resource.RenameEpisodes,
            ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
            MultiEpisodeStyle = (MultiEpisodeStyle)resource.MultiEpisodeStyle,
            ColonReplacementFormat = (ColonReplacementFormat)resource.ColonReplacementFormat,
            CustomColonReplacementFormat = resource.CustomColonReplacementFormat ?? "",
            StandardEpisodeFormat = resource.StandardEpisodeFormat,
            DailyEpisodeFormat = resource.DailyEpisodeFormat,
            AnimeEpisodeFormat = resource.AnimeEpisodeFormat,
            GameFolderFormat = resource.GameFolderFormat,
            PlatformFolderFormat = resource.PlatformFolderFormat,
            SpecialsFolderFormat = resource.SpecialsFolderFormat
        };
    }
}
