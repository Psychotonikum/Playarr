using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(190)]
    public class health_restored_notification : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnHealthRestored").AsBoolean().WithDefaultValue(false);
        }
    }
}
