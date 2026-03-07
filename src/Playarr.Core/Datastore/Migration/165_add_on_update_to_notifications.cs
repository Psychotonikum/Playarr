using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(165)]
    public class add_on_update_to_notifications : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnApplicationUpdate").AsBoolean().WithDefaultValue(false);
        }
    }
}
