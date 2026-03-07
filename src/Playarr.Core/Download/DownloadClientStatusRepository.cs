using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.Download
{
    public interface IDownloadClientStatusRepository : IProviderStatusRepository<DownloadClientStatus>
    {
    }

    public class DownloadClientStatusRepository : ProviderStatusRepository<DownloadClientStatus>, IDownloadClientStatusRepository
    {
        public DownloadClientStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
