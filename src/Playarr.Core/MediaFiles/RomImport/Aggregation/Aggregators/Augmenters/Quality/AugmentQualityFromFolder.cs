using Playarr.Core.Download;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFolder : IAugmentQuality
    {
        public int Order => 2;
        public string Name => "FolderName";

        public AugmentQualityResult AugmentQuality(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var quality = localRom.FolderRomInfo?.Quality;

            if (quality == null)
            {
                return null;
            }

            var sourceConfidence = quality.SourceDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;

            var resolutionConfidence = quality.ResolutionDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;

            var revisionConfidence = quality.RevisionDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;

            return new AugmentQualityResult(quality.Quality.Source,
                                            sourceConfidence,
                                            quality.Quality.Resolution,
                                            resolutionConfidence,
                                            quality.Revision,
                                            revisionConfidence);
        }
    }
}
