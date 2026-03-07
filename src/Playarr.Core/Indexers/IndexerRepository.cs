using System.Linq;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Indexers
{
    public interface IIndexerRepository : IProviderRepository<IndexerDefinition>
    {
        IndexerDefinition FindByName(string name);
    }

    public class IndexerRepository : ProviderRepository<IndexerDefinition>, IIndexerRepository
    {
        public IndexerRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public IndexerDefinition FindByName(string name)
        {
            return Query(i => i.Name == name).SingleOrDefault();
        }
    }
}
