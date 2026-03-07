using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(209)]
    public class add_on_import_complete_to_notifications : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnImportComplete").AsBoolean().WithDefaultValue(false);
        }
    }
}
