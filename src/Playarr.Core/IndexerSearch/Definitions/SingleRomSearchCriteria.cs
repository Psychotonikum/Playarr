using System.Linq;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class SingleEpisodeSearchCriteria : SearchCriteriaBase
    {
        public int EpisodeNumber { get; set; }
        public int PlatformNumber { get; set; }

        public override string ToString()
        {
            var platformName = Game.Platforms?.FirstOrDefault(p => p.PlatformNumber == PlatformNumber)?.Title ?? $"Platform {PlatformNumber}";
            var romTitle = Roms?.FirstOrDefault()?.Title ?? $"ROM {EpisodeNumber}";

            return $"[{Game.Title} | {platformName} - {romTitle}]";
        }
    }
}
