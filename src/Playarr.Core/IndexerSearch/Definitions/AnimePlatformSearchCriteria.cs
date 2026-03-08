namespace Playarr.Core.IndexerSearch.Definitions
{
    public class AnimeSeasonSearchCriteria : SearchCriteriaBase
    {
        public int PlatformNumber { get; set; }

        public override string ToString()
        {
            return $"[{Game.Title} : S{PlatformNumber:00}]";
        }
    }
}
