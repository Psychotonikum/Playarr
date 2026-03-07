using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(173)]
    public class remove_omg : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DELETE FROM \"Indexers\" WHERE \"Implementation\" = 'Omgwtfnzbs'");
        }
    }
}
