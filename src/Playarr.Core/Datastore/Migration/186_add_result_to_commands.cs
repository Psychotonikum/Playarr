using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(186)]
    public class add_result_to_commands : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Commands").AddColumn("Result").AsInt32().WithDefaultValue(1);
        }
    }
}
