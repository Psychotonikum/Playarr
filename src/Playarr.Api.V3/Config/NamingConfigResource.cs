using Playarr.Http.REST;

namespace Playarr.Api.V3.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int ColonReplacementFormat { get; set; }
        public string CustomColonReplacementFormat { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
        public string AnimeEpisodeFormat { get; set; }
        public string GameFolderFormat { get; set; }
        public string PlatformFolderFormat { get; set; }
        public string SpecialsFolderFormat { get; set; }
    }
}
