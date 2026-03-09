using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.MetadataSource.Providers
{
    public interface IMetadataSourceProviderRepository : IProviderRepository<MetadataSourceDefinition>
    {
    }

    public class MetadataSourceProviderRepository : ProviderRepository<MetadataSourceDefinition>, IMetadataSourceProviderRepository
    {
        public MetadataSourceProviderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
