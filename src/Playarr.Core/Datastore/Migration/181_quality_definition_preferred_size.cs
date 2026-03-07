using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(181)]
    public class quality_definition_preferred_size : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("QualityDefinitions").AddColumn("PreferredSize").AsDouble().Nullable();

            Execute.Sql("UPDATE \"QualityDefinitions\" SET \"PreferredSize\" = \"MaxSize\" - 5 WHERE \"MaxSize\" > 5");
        }
    }
}
