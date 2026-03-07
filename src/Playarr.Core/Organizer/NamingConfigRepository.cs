using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Organizer
{
    public interface INamingConfigRepository : IBasicRepository<NamingConfig>
    {
    }

    public class NamingConfigRepository : BasicRepository<NamingConfig>, INamingConfigRepository
    {
        public NamingConfigRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
