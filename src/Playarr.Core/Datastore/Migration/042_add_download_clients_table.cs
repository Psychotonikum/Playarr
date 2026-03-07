using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(42)]
    public class add_download_clients_table : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("DownloadClients")
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Implementation").AsString().NotNullable()
                  .WithColumn("Settings").AsString().NotNullable()
                  .WithColumn("ConfigContract").AsString().NotNullable()
                  .WithColumn("Protocol").AsInt32().NotNullable();
        }
    }
}
