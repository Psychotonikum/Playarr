using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(134)]
    public class add_specials_folder_format : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SpecialsFolderFormat").AsString().Nullable();

            Update.Table("NamingConfig").Set(new { SpecialsFolderFormat = "Specials" }).AllRows();
        }
    }
}
