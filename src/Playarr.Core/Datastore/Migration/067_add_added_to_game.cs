using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(67)]
    public class add_added_to_series : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("Added").AsDateTime().Nullable();
        }
    }
}
