using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(94)]
    public class add_tvmazeid : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("RawgId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("RawgId");
        }
    }
}
