using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseHash : IAggregateLocalEpisode
    {
        public int Order => 1;

        public LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var releaseHash = GetReleaseHash(localRom.FileRomInfo);

            if (releaseHash.IsNullOrWhiteSpace())
            {
                releaseHash = GetReleaseHash(localRom.DownloadClientRomInfo);
            }

            if (releaseHash.IsNullOrWhiteSpace())
            {
                releaseHash = GetReleaseHash(localRom.FolderRomInfo);
            }

            localRom.ReleaseHash = releaseHash;

            return localRom;
        }

        private string GetReleaseHash(ParsedRomInfo romInfo)
        {
            // ReleaseHash doesn't make sense for a FullSeason, since hashes should be specific to a file
            if (romInfo == null || romInfo.FullSeason)
            {
                return null;
            }

            return romInfo.ReleaseHash;
        }
    }
}
