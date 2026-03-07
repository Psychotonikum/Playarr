using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Profiles.Releases
{
    public interface IRestrictionRepository : IBasicRepository<ReleaseProfile>
    {
    }

    public class ReleaseProfileRepository : BasicRepository<ReleaseProfile>, IRestrictionRepository
    {
        public ReleaseProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
