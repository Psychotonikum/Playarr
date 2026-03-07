using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.AutoTagging
{
    public interface IAutoTaggingRepository : IBasicRepository<AutoTag>
    {
    }

    public class AutoTaggingRepository : BasicRepository<AutoTag>, IAutoTaggingRepository
    {
        public AutoTaggingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
