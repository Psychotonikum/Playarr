using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBlocklist : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedBlocklist(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""Blocklist""
                                     WHERE ""Id"" IN (
                                     SELECT ""Blocklist"".""Id"" FROM ""Blocklist""
                                     LEFT OUTER JOIN ""Games""
                                     ON ""Blocklist"".""GameId"" = ""Games"".""Id""
                                     WHERE ""Games"".""Id"" IS NULL)");
        }
    }
}
