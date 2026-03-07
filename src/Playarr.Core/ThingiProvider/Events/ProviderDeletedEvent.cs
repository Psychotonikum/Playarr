using Playarr.Common.Messaging;

namespace Playarr.Core.ThingiProvider.Events
{
    public class ProviderDeletedEvent<TProvider> : IEvent
    {
        public int ProviderId { get; private set; }

        public ProviderDeletedEvent(int id)
        {
            ProviderId = id;
        }
    }
}
