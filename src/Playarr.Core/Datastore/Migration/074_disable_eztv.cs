using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(74)]
    public class disable_eztv : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false WHERE \"Implementation\" = 'Eztv' AND \"Settings\" LIKE '%ezrss.it%'");
        }
    }
}
