using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(177)]
    public class add_on_manual_interaction_required_to_notifications : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnManualInteractionRequired").AsBoolean().WithDefaultValue(false);
        }
    }
}
