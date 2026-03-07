using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(35)]
    public class add_series_folder_format_to_naming_config : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("GameFolderFormat").AsString().Nullable();

            Update.Table("NamingConfig").Set(new { GameFolderFormat = "{Game Title}" }).AllRows();
        }
    }
}
