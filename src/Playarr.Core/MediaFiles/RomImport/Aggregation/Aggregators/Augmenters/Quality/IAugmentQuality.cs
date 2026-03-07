using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public interface IAugmentQuality
    {
        int Order { get; }
        string Name { get; }
        AugmentQualityResult AugmentQuality(LocalEpisode localRom, DownloadClientItem downloadClientItem);
    }
}
