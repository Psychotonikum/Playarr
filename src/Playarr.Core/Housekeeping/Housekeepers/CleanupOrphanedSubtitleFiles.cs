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
                                     LEFT OUTER JOIN ""Series""
                                     ON ""SubtitleFiles"".""GameId"" = ""Series"".""Id""
                                     WHERE ""Series"".""Id"" IS NULL)");
        }

        private void DeleteOrphanedByRomFile()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""SubtitleFiles"".""Id"" FROM ""SubtitleFiles""
                                     LEFT OUTER JOIN ""EpisodeFiles""
                                     ON ""SubtitleFiles"".""EpisodeFileId"" = ""EpisodeFiles"".""Id""
                                     WHERE ""SubtitleFiles"".""EpisodeFileId"" > 0
                                     AND ""EpisodeFiles"".""Id"" IS NULL)");
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
