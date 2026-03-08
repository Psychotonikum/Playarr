using Dapper;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedRomFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedRomFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""RomFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""RomFiles"".""Id"" FROM ""RomFiles""
                                     LEFT OUTER JOIN ""Roms""
                                     ON ""RomFiles"".""Id"" = ""Roms"".""EpisodeFileId""
                                     WHERE ""Roms"".""Id"" IS NULL)");
        }
    }
}
