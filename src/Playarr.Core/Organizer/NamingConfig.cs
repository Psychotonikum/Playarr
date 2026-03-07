using Playarr.Core.Datastore;

namespace Playarr.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameEpisodes = false,
            ReplaceIllegalCharacters = true,
            ColonReplacementFormat = ColonReplacementFormat.Smart,
            CustomColonReplacementFormat = string.Empty,
            MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange,
            StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}",
            DailyEpisodeFormat = "{Game Title} - {Air-Date} - {Rom Title} {Quality Full}",
            AnimeEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}",
            GameFolderFormat = "{Game Title}",
            PlatformFolderFormat = "Platform {platform}",
            SpecialsFolderFormat = "Specials"
        };

        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public string CustomColonReplacementFormat { get; set; }
        public MultiEpisodeStyle MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
        public string AnimeEpisodeFormat { get; set; }
        public string GameFolderFormat { get; set; }
        public string PlatformFolderFormat { get; set; }
        public string SpecialsFolderFormat { get; set; }
    }
}
