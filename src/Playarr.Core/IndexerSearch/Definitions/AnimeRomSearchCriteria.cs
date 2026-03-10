using System.Linq;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class AnimeEpisodeSearchCriteria : SearchCriteriaBase
    {
        public int AbsoluteEpisodeNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int PlatformNumber { get; set; }
        public bool IsSeasonSearch { get; set; }

        public override string ToString()
        {
            var platformName = Game.Platforms?.FirstOrDefault(p => p.PlatformNumber == PlatformNumber)?.Title ?? $"Platform {PlatformNumber}";
            var romTitle = Roms?.FirstOrDefault()?.Title ?? $"ROM {EpisodeNumber}";

            return $"[{Game.Title} | {platformName} - {romTitle}]";
        }
    }
}
