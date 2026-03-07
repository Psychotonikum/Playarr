using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(219)]
    public class nzb_su_url_to_nzb_life : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = replace(\"Settings\", '//api.nzb.su', '//api.nzb.life')" +
                        "WHERE \"Implementation\" = 'Newznab'" +
                        "AND \"Settings\" LIKE '%//api.nzb.su%'");
        }
    }
}
