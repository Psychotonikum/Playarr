using Playarr.Common.Messaging;

namespace Playarr.Core.Messaging.Events
{
    public interface IEventAggregator
    {
        void PublishEvent<TEvent>(TEvent @event)
            where TEvent : class,  IEvent;
    }
}
