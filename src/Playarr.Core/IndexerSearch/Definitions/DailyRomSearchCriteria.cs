using System;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class DailyEpisodeSearchCriteria : SearchCriteriaBase
    {
        public DateTime AirDate { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1:yyyy-MM-dd}]", Game.Title, AirDate);
        }
    }
}
