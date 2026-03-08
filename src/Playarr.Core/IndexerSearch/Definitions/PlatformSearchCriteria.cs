namespace Playarr.Core.IndexerSearch.Definitions
{
    public class SeasonSearchCriteria : SearchCriteriaBase
    {
        public int PlatformNumber { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : S{1:00}]", Game.Title, PlatformNumber);
        }
    }
}
