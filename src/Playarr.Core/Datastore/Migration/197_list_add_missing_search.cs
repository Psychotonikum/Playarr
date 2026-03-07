using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(197)]
    public class list_add_missing_search : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("SearchForMissingEpisodes").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}
