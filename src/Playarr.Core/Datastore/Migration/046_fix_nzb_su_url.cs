using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(46)]
    public class fix_nzb_su_url : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = replace(\"Settings\", '//nzb.su', '//api.nzb.su')" +
                        "WHERE \"Implementation\" = 'Newznab'" +
                        "AND \"Settings\" LIKE '%//nzb.su%'");
        }
    }
}
