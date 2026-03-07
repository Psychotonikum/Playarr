using Playarr.Common.Messaging;
using Playarr.Core.Download;

namespace Playarr.Core.Indexers
{
    public class RssSyncCompleteEvent : IEvent
    {
        public ProcessedDecisions ProcessedDecisions { get; private set; }

        public RssSyncCompleteEvent(ProcessedDecisions processedDecisions)
        {
            ProcessedDecisions = processedDecisions;
        }
    }
}
