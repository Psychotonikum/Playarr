namespace Playarr.Core.IndexerSearch.Definitions
{
    public class AnimeSeasonSearchCriteria : SearchCriteriaBase
    {
        public int SeasonNumber { get; set; }

        public override string ToString()
        {
            return $"[{Game.Title} : S{SeasonNumber:00}]";
        }
    }
}
