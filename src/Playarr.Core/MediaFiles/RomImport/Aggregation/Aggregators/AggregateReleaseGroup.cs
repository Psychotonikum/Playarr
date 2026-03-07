using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregateLocalEpisode
    {
        public int Order => 1;

        public LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            // Prefer ReleaseGroup from DownloadClient/Folder if they're not a platform pack
            var releaseGroup = GetReleaseGroup(localRom.DownloadClientRomInfo, true);

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.FolderRomInfo, true);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.FileRomInfo, false);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.DownloadClientRomInfo, false);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localRom.FolderRomInfo, false);
            }

            localRom.ReleaseGroup = releaseGroup;

            return localRom;
        }

        private string GetReleaseGroup(ParsedRomInfo romInfo, bool skipFullSeason)
        {
            if (romInfo == null || (romInfo.FullSeason && skipFullSeason))
            {
                return null;
            }

            return romInfo.ReleaseGroup;
        }
    }
}
