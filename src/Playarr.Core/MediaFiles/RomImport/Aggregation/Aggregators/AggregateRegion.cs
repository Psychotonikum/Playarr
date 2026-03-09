using Playarr.Core.Download;
using Playarr.Core.Games;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateRegion : IAggregateLocalEpisode
    {
        public int Order => 3;

        public LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var fileName = System.IO.Path.GetFileName(localRom.Path);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return localRom;
            }

            var parsed = NoIntroFileNameParser.Parse(fileName);

            if (parsed == null)
            {
                return localRom;
            }

            localRom.Region = parsed.Region.ToString();
            localRom.Revision = parsed.Revision;
            localRom.DumpQuality = (int)parsed.DumpQuality;
            localRom.Modification = (int)parsed.Modification;
            localRom.ModificationName = parsed.ModificationName;
            localRom.RomReleaseType = (int)parsed.ReleaseType;

            return localRom;
        }
    }
}
