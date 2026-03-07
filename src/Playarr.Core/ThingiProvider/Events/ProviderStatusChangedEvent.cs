using Playarr.Common.Messaging;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.ThingiProvider.Events
{
    public class ProviderStatusChangedEvent<TProvider> : IEvent
    {
        public int ProviderId { get; private set; }

        public ProviderStatusBase Status { get; private set; }

        public ProviderStatusChangedEvent(int id, ProviderStatusBase status)
        {
            ProviderId = id;
            Status = status;
        }
    }
}
