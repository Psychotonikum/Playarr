using System;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Instrumentation
{
    public interface ILogRepository : IBasicRepository<Log>
    {
        void Trim();
    }

    public class LogRepository : BasicRepository<Log>, ILogRepository
    {
        public LogRepository(ILogDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void Trim()
        {
            var trimDate = DateTime.UtcNow.AddDays(-7).Date;
            Delete(c => c.Time <= trimDate);
            Vacuum();
        }
    }
}
