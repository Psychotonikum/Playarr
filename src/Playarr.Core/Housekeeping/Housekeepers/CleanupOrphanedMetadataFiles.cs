using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMetadataFiles(IMainDatabase database)
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
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""MetadataFiles"".""Id"" FROM ""MetadataFiles""
                                     LEFT OUTER JOIN ""Series""
                                     ON ""MetadataFiles"".""SeriesId"" = ""Series"".""Id""
                                     WHERE ""Series"".""Id"" IS NULL)");
        }

        private void DeleteOrphanedByRomFile()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""MetadataFiles"".""Id"" FROM ""MetadataFiles""
                                     LEFT OUTER JOIN ""EpisodeFiles""
                                     ON ""MetadataFiles"".""EpisodeFileId"" = ""EpisodeFiles"".""Id""
                                     WHERE ""MetadataFiles"".""EpisodeFileId"" > 0
                                     AND ""EpisodeFiles"".""Id"" IS NULL)");
        }

        private void DeleteWhereRomFileIsZero()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""Id"" FROM ""MetadataFiles""
                                     WHERE ""Type"" IN (2, 5)
                                     AND ""EpisodeFileId"" = 0)");
        }
    }
}
