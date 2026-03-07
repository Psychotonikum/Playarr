using System.Linq;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class SpecialEpisodeSearchCriteria : SearchCriteriaBase
    {
        public string[] EpisodeQueryTitles { get; set; }

        public override string ToString()
        {
            var romTitles = EpisodeQueryTitles.ToList();

            if (romTitles.Count > 0)
            {
                return $"[{Game.Title} ({Game.SeriesType})] Specials";
            }

            return $"[{Game.Title} ({Game.SeriesType}): {string.Join(",", EpisodeQueryTitles)}]";
        }
    }
}
