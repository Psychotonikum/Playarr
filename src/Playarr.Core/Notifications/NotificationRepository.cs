using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Notifications
{
    public interface INotificationRepository : IProviderRepository<NotificationDefinition>
    {
        void UpdateSettings(NotificationDefinition model);
    }

    public class NotificationRepository : ProviderRepository<NotificationDefinition>, INotificationRepository
    {
        public NotificationRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void UpdateSettings(NotificationDefinition model)
        {
            SetFields(model, m => m.Settings);
        }
    }
}
