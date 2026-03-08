using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedExtraFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedExtraFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteOrphanedBySeries();
            DeleteOrphanedByRomFile();
            DeleteWhereRomFileIsZero();
        }

        private void DeleteOrphanedBySeries()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""ExtraFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""ExtraFiles"".""Id"" FROM ""ExtraFiles""
                                     LEFT OUTER JOIN ""Games""
                                     ON ""ExtraFiles"".""GameId"" = ""Games"".""Id""
                                     WHERE ""Games"".""Id"" IS NULL)");
        }

        private void DeleteOrphanedByRomFile()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""ExtraFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""ExtraFiles"".""Id"" FROM ""ExtraFiles""
                                     LEFT OUTER JOIN ""RomFiles""
                                     ON ""ExtraFiles"".""EpisodeFileId"" = ""RomFiles"".""Id""
                                     WHERE ""ExtraFiles"".""EpisodeFileId"" > 0
                                     AND ""RomFiles"".""Id"" IS NULL)");
        }

        private void DeleteWhereRomFileIsZero()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""ExtraFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""Id"" FROM ""ExtraFiles""
                                     WHERE ""EpisodeFileId"" = 0)");
        }
    }
}
