using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public interface IAugmentLanguage
    {
        int Order { get; }
        string Name { get; }
        AugmentLanguageResult AugmentLanguage(LocalEpisode localRom, DownloadClientItem downloadClientItem);
    }
}
