using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(21)]
    public class drop_seasons_table : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("Platforms");
        }
    }
}
