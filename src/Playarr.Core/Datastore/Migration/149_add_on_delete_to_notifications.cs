using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(149)]
    public class add_on_delete_to_notifications : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnSeriesDelete").AsBoolean().WithDefaultValue(false);
            Alter.Table("Notifications").AddColumn("OnRomFileDelete").AsBoolean().WithDefaultValue(false);
        }
    }
}
