using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesRefreshStartingEvent : IEvent
    {
        public bool ManualTrigger { get; set; }

        public SeriesRefreshStartingEvent(bool manualTrigger)
        {
            ManualTrigger = manualTrigger;
        }
    }
}
