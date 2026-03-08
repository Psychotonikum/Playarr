using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedHistoryItems : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedHistoryItems(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            CleanupOrphanedBySeries();
            CleanupOrphanedByEpisode();
        }

        private void CleanupOrphanedBySeries()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""History""
                                     WHERE ""Id"" IN (
                                     SELECT ""History"".""Id"" FROM ""History""
                                     LEFT OUTER JOIN ""Games""
                                     ON ""History"".""GameId"" = ""Games"".""Id""
                                     WHERE ""Games"".""Id"" IS NULL)");
        }

        private void CleanupOrphanedByEpisode()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""History""
                                     WHERE ""Id"" IN (
                                     SELECT ""History"".""Id"" FROM ""History""
                                     LEFT OUTER JOIN ""Roms""
                                     ON ""History"".""EpisodeId"" = ""Roms"".""Id""
                                     WHERE ""Roms"".""Id"" IS NULL)");
        }
    }
}
