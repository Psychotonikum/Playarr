using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.History;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseInfo : IAggregateLocalEpisode
    {
        public int Order => 1;

        private readonly IHistoryService _historyService;

        public AggregateReleaseInfo(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        public LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                return localRom;
            }

            var grabbedHistories = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == EpisodeHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistories.Empty())
            {
                return localRom;
            }

            localRom.Release = new GrabbedReleaseInfo(grabbedHistories);

            return localRom;
        }
    }
}
