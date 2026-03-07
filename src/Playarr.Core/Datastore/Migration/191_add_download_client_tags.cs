using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(191)]
    public class add_download_client_tags : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DownloadClients").AddColumn("Tags").AsString().Nullable();
        }
    }
}
