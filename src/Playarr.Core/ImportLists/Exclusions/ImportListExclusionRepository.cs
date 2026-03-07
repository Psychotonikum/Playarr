using System.Linq;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        ImportListExclusion FindByIgdbId(int igdbId);
    }

    public class ImportListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public ImportListExclusion FindByIgdbId(int igdbId)
        {
            return Query(m => m.TvdbId == igdbId).SingleOrDefault();
        }
    }
}
