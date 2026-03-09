using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(228)]
    public class add_game_systems : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("GameSystems")
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("FolderName").AsString().NotNullable()
                .WithColumn("SystemType").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("FileExtensions").AsString().Nullable()
                .WithColumn("NamingFormat").AsString().Nullable()
                .WithColumn("UpdateNamingFormat").AsString().Nullable()
                .WithColumn("DlcNamingFormat").AsString().Nullable()
                .WithColumn("BaseFolderName").AsString().Nullable()
                .WithColumn("UpdateFolderName").AsString().Nullable()
                .WithColumn("DlcFolderName").AsString().Nullable()
                .WithColumn("Tags").AsString().Nullable();

            // Add GameSystemId column to Series table
            Alter.Table("Series")
                .AddColumn("GameSystemId").AsInt32().Nullable();

            // Add RomFileType column to EpisodeFiles table
            Alter.Table("EpisodeFiles")
                .AddColumn("RomFileType").AsInt32().NotNullable().WithDefaultValue(0)
                .AddColumn("PatchVersion").AsString().Nullable()
                .AddColumn("DlcIndex").AsString().Nullable()
                .AddColumn("LinkedGameId").AsInt32().Nullable();

            // Game systems start empty — users add them via Settings > Game Systems using presets
        }
    }
}
