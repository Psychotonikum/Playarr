using System.Collections.Generic;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;

namespace Playarr.Core.Blocklisting
{
    public interface IBlocklistRepository : IBasicRepository<Blocklist>
    {
        List<Blocklist> BlocklistedByTitle(int gameId, string sourceTitle);
        List<Blocklist> BlocklistedByTorrentInfoHash(int gameId, string torrentInfoHash);
        List<Blocklist> BlocklistedBySeries(int gameId);
        void DeleteForGameIds(List<int> gameIds);
    }

    public class BlocklistRepository : BasicRepository<Blocklist>, IBlocklistRepository
    {
        public BlocklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blocklist> BlocklistedByTitle(int gameId, string sourceTitle)
        {
            return Query(e => e.SeriesId == gameId && e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blocklist> BlocklistedByTorrentInfoHash(int gameId, string torrentInfoHash)
        {
            return Query(e => e.SeriesId == gameId && e.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blocklist> BlocklistedBySeries(int gameId)
        {
            return Query(b => b.SeriesId == gameId);
        }

        public void DeleteForGameIds(List<int> gameIds)
        {
            Delete(x => gameIds.Contains(x.SeriesId));
        }

        public override PagingSpec<Blocklist> GetPaged(PagingSpec<Blocklist> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(PagedBuilder(), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(Blocklist))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(PagedBuilder().Select(typeof(Blocklist)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        protected override SqlBuilder PagedBuilder()
        {
            var builder = Builder()
                .Join<Blocklist, Game>((b, m) => b.SeriesId == m.Id);

            return builder;
        }

        protected override IEnumerable<Blocklist> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<Blocklist, Game>(builder, (blocklist, game) =>
            {
                blocklist.Game = game;
                return blocklist;
            });
    }
}
