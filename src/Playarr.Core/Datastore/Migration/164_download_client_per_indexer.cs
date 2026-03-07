using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(164)]
    public class download_client_per_indexer : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("DownloadClientId").AsInt32().WithDefaultValue(0);
        }
    }
}
