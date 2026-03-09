using System.Linq;
using NLog;
using Playarr.Common.Instrumentation;

namespace Playarr.Core.Qualities
{
    public static class QualityFinder
    {
        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(QualityFinder));

        public static Quality FindBySourceAndResolution(QualitySource source, int resolution)
        {
            // For ROM quality, source-based lookup is the primary mechanism
            if (source == QualitySource.CRC)
            {
                // resolution field repurposed: 0 = bad, 1 = verified
                return resolution > 0 ? Quality.Verified : Quality.Bad;
            }

            var matchingQuality = Quality.All.SingleOrDefault(q => q.Source == source && q.Resolution == resolution);

            if (matchingQuality != null)
            {
                return matchingQuality;
            }

            Logger.Warn("Unable to find exact quality for {0} and {1}. Using Unknown as fallback", source, resolution);

            return Quality.Unknown;
        }
    }
}
