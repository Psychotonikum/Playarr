using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.Notifications
{
    public interface INotificationStatusRepository : IProviderStatusRepository<NotificationStatus>
    {
    }

    public class NotificationStatusRepository : ProviderStatusRepository<NotificationStatus>, INotificationStatusRepository
    {
        public NotificationStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
