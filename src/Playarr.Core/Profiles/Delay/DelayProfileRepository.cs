using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Profiles.Delay
{
    public interface IDelayProfileRepository : IBasicRepository<DelayProfile>
    {
    }

    public class DelayProfileRepository : BasicRepository<DelayProfile>, IDelayProfileRepository
    {
        public DelayProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
