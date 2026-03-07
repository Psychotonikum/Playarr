using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(126)]
    public class add_custom_filters : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("CustomFilters")
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Label").AsString().NotNullable()
                  .WithColumn("Filters").AsString().NotNullable();
        }
    }
}
