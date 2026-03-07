using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(106)]
    public class update_btn_url : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = Replace(\"Settings\", 'api.btnapps.net', 'api.broadcasthe.net') WHERE \"Implementation\" = 'BroadcastheNet';");
        }
    }

    public class BroadcastheNetSettings106
    {
        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }
    }
}
