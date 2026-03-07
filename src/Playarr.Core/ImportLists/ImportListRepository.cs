using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.ImportLists
{
    public interface IImportListRepository : IProviderRepository<ImportListDefinition>
    {
        void UpdateSettings(ImportListDefinition model);
    }

    public class ImportListRepository : ProviderRepository<ImportListDefinition>, IImportListRepository
    {
        public ImportListRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void UpdateSettings(ImportListDefinition model)
        {
            SetFields(model, m => m.Settings);
        }
    }
}
