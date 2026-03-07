using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Download
{
    public interface IDownloadClientRepository : IProviderRepository<DownloadClientDefinition>
    {
    }

    public class DownloadClientRepository : ProviderRepository<DownloadClientDefinition>, IDownloadClientRepository
    {
        public DownloadClientRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
