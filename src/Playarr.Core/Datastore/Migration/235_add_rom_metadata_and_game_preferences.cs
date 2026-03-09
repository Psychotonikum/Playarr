using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(235)]
    public class add_rom_metadata_and_game_preferences : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // New ROM metadata fields on RomFiles
            Alter.Table("RomFiles").AddColumn("Revision").AsString().Nullable();
            Alter.Table("RomFiles").AddColumn("DumpQuality").AsInt32().WithDefaultValue(0);
            Alter.Table("RomFiles").AddColumn("Modification").AsInt32().WithDefaultValue(0);
            Alter.Table("RomFiles").AddColumn("ModificationName").AsString().Nullable();
            Alter.Table("RomFiles").AddColumn("RomReleaseType").AsInt32().WithDefaultValue(0);

            // Game-level preference fields (stored as JSON arrays)
            Alter.Table("Games").AddColumn("PreferredRegions").AsString().Nullable().WithDefaultValue("[]");
            Alter.Table("Games").AddColumn("PreferredLanguageIds").AsString().Nullable().WithDefaultValue("[]");
            Alter.Table("Games").AddColumn("PreferredReleaseTypes").AsString().Nullable().WithDefaultValue("[]");
            Alter.Table("Games").AddColumn("PreferredModifications").AsString().Nullable().WithDefaultValue("[]");
        }
    }
}
