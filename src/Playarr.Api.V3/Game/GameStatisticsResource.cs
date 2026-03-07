using System.Collections.Generic;
using System.Linq;
using Playarr.Core.GameStats;

namespace Playarr.Api.V3.Game
{
    public class SeriesStatisticsResource
    {
        public int SeasonCount { get; set; }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }
        public int TotalEpisodeCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }

        public decimal PercentOfEpisodes
        {
            get
            {
                if (EpisodeCount == 0)
                {
                    return 0;
                }

                return (decimal)EpisodeFileCount / (decimal)EpisodeCount * 100;
            }
        }
    }

    public static class SeriesStatisticsResourceMapper
    {
        public static SeriesStatisticsResource ToResource(this SeriesStatistics model, List<SeasonResource> platforms)
        {
            if (model == null)
            {
                return null;
            }

            return new SeriesStatisticsResource
            {
                SeasonCount = platforms == null ? 0 : platforms.Where(s => s.SeasonNumber > 0).Count(),
                EpisodeFileCount = model.EpisodeFileCount,
                EpisodeCount = model.EpisodeCount,
                TotalEpisodeCount = model.TotalEpisodeCount,
                SizeOnDisk = model.SizeOnDisk,
                ReleaseGroups = model.ReleaseGroups
            };
        }
    }
}
