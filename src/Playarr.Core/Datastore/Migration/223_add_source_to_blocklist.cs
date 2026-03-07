using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(223)]
    public class add_source_to_blocklist : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blocklist").AddColumn("Source").AsString().Nullable();
        }
    }
}
