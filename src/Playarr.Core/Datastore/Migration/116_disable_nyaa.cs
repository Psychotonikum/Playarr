using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(116)]
    public class disable_nyaa : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false, \"Settings\" = Replace(\"Settings\", 'https://nyaa.se', '') WHERE \"Implementation\" = 'Nyaa' AND \"Settings\" LIKE '%nyaa.se%';");
        }
    }
}
