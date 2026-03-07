using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public interface IAggregateLocalEpisode
    {
        int Order { get; }
        LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem);
    }
}
