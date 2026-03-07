using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Extras.Metadata
{
    public interface IMetadataRepository : IProviderRepository<MetadataDefinition>
    {
    }

    public class MetadataRepository : ProviderRepository<MetadataDefinition>, IMetadataRepository
    {
        public MetadataRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
