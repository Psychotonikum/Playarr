using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.Indexers
{
    public interface IIndexerStatusRepository : IProviderStatusRepository<IndexerStatus>
    {
    }

    public class IndexerStatusRepository : ProviderStatusRepository<IndexerStatus>, IIndexerStatusRepository
    {
        public IndexerStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
