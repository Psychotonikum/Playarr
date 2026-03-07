using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(143)]
    public class add_priority_to_indexers : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("Priority").AsInt32().NotNullable().WithDefaultValue(25);
        }
    }
}
