using System.Collections.Generic;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;

namespace Playarr.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteByGameIds(List<int> gameIds);
        List<PendingRelease> AllByGameId(int gameId);
        List<PendingRelease> WithoutFallback();
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteByGameIds(List<int> gameIds)
        {
            Delete(r => gameIds.Contains(r.GameId));
        }

        public List<PendingRelease> AllByGameId(int gameId)
        {
            return Query(p => p.GameId == gameId);
        }

        public List<PendingRelease> WithoutFallback()
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .InnerJoin<PendingRelease, Game>((p, s) => p.GameId == s.Id)
                .Where<PendingRelease>(p => p.Reason != PendingReleaseReason.Fallback);

            return Query(builder);
        }
    }
}
