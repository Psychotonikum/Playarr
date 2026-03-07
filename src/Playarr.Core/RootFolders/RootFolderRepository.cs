using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {
    }

    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;
    }
}
