using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.CustomFilters
{
    public interface ICustomFilterRepository : IBasicRepository<CustomFilter>
    {
    }

    public class CustomFilterRepository : BasicRepository<CustomFilter>, ICustomFilterRepository
    {
        public CustomFilterRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
