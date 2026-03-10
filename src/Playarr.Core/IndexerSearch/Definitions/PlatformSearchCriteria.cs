using System.Linq;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class SeasonSearchCriteria : SearchCriteriaBase
    {
        public int PlatformNumber { get; set; }

        public override string ToString()
        {
            var platformName = Game.Platforms?.FirstOrDefault(p => p.PlatformNumber == PlatformNumber)?.Title ?? $"Platform {PlatformNumber}";

            return $"[{Game.Title} | {platformName}]";
        }
    }
}
