using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(152)]
    public class update_btn_url_to_https : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = Replace(\"Settings\", 'http://api.broadcasthe.net', 'https://api.broadcasthe.net') WHERE \"Implementation\" = 'BroadcastheNet';");
        }
    }
}
