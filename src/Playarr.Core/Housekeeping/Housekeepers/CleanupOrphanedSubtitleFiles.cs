using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedSubtitleFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedSubtitleFiles(IMainDatabase database)
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
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""SubtitleFiles"".""Id"" FROM ""SubtitleFiles""
                                     LEFT OUTER JOIN ""Games""
                                     ON ""SubtitleFiles"".""GameId"" = ""Games"".""Id""
                                     WHERE ""Games"".""Id"" IS NULL)");
        }

        private void DeleteOrphanedByRomFile()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""SubtitleFiles"".""Id"" FROM ""SubtitleFiles""
                                     LEFT OUTER JOIN ""RomFiles""
                                     ON ""SubtitleFiles"".""EpisodeFileId"" = ""RomFiles"".""Id""
                                     WHERE ""SubtitleFiles"".""EpisodeFileId"" > 0
                                     AND ""RomFiles"".""Id"" IS NULL)");
        }

        private void DeleteWhereRomFileIsZero()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""Id"" FROM ""SubtitleFiles""
                                     WHERE ""EpisodeFileId"" = 0)");
        }
    }
}
