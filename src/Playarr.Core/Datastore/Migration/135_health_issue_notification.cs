using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(135)]
    public class health_issue_notification : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnHealthIssue").AsBoolean().WithDefaultValue(false);
            Alter.Table("Notifications").AddColumn("IncludeHealthWarnings").AsBoolean().WithDefaultValue(false);
        }
    }
}
