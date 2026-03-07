using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(97)]
    public class add_reason_to_pending_releases : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases").AddColumn("Reason").AsInt32().WithDefaultValue(0);
        }
    }
}
