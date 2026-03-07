using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(178)]
    public class list_sync_time : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("LastSyncListInfo").FromTable("ImportListStatus");

            Alter.Table("ImportListStatus").AddColumn("LastInfoSync").AsDateTime().Nullable();
        }
    }
}
