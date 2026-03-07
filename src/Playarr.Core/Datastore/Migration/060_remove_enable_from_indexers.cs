using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(60)]
    public class remove_enable_from_indexers : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Enable").FromTable("Indexers");
            Delete.Column("Protocol").FromTable("DownloadClients");
        }
    }
}
