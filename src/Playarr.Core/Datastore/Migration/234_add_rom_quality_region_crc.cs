using System.Data;
using Dapper;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(234)]
    public class add_rom_quality_region_crc : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("RomFiles").AddColumn("Region").AsString().Nullable();
            Alter.Table("RomFiles").AddColumn("CrcHash").AsString().Nullable();

            // Reset quality definitions and profiles to ROM-appropriate values
            Execute.WithConnection(ResetQualityDefinitions);
            Execute.WithConnection(ResetQualityProfiles);
        }

        private void ResetQualityDefinitions(IDbConnection conn, IDbTransaction tran)
        {
            // Remove old TV-oriented quality definitions
            conn.Execute("DELETE FROM \"QualityDefinitions\"", transaction: tran);

            // Insert ROM-appropriate quality definitions (table only has Id, Quality, Title)
            conn.Execute(
                "INSERT INTO \"QualityDefinitions\" (\"Quality\", \"Title\") VALUES (@Quality, @Title)",
                new[]
                {
                    new { Quality = 0, Title = "Unknown" },
                    new { Quality = 1, Title = "Bad" },
                    new { Quality = 2, Title = "Verified" }
                },
                transaction: tran);
        }

        private void ResetQualityProfiles(IDbConnection conn, IDbTransaction tran)
        {
            // Reset any existing quality profiles to use the new ROM quality values
            // The profile stores Items as JSON array of quality items
            var romItems = "[{\"quality\":0,\"items\":[],\"allowed\":true},{\"quality\":1,\"items\":[],\"allowed\":false},{\"quality\":2,\"items\":[],\"allowed\":true}]";

            conn.Execute(
                "UPDATE \"QualityProfiles\" SET \"Items\" = @Items, \"Cutoff\" = 2",
                new { Items = romItems },
                transaction: tran);
        }
    }
}
