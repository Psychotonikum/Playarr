using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodes : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedEpisodes(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""Roms""
                                     WHERE ""Id"" IN (
                                     SELECT ""Roms"".""Id"" FROM ""Roms""
                                     LEFT OUTER JOIN ""Games""
                                     ON ""Roms"".""GameId"" = ""Games"".""Id""
                                     WHERE ""Games"".""Id"" IS NULL)");
        }
    }
}
