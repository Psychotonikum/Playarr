using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(125)]
    public class remove_notify_my_android_and_pushalot_notifications : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "NotifyMyAndroid" });
            Delete.FromTable("Notifications").Row(new { Implementation = "Pushalot" });
        }
    }
}
