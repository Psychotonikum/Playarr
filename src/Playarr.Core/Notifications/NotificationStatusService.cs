using System;
using NLog;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.Notifications
{
    public interface INotificationStatusService : IProviderStatusServiceBase<NotificationStatus>
    {
    }

    public class NotificationStatusService : ProviderStatusServiceBase<INotification, NotificationStatus>, INotificationStatusService
    {
        public NotificationStatusService(INotificationStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
            MinimumTimeSinceInitialFailure = TimeSpan.FromMinutes(5);
            MaximumEscalationLevel = 5;
        }
    }
}
