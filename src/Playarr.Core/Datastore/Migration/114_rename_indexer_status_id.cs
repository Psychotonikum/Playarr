using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(114)]
    public class rename_indexer_status_id : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("IndexerId").OnTable("IndexerStatus").To("ProviderId");
        }
    }
}
