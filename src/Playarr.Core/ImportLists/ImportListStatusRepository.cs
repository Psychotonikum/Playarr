using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.ImportLists
{
    public interface IImportListStatusRepository : IProviderStatusRepository<ImportListStatus>
    {
    }

    public class ImportListStatusRepository : ProviderStatusRepository<ImportListStatus>, IImportListStatusRepository
    {
        public ImportListStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
