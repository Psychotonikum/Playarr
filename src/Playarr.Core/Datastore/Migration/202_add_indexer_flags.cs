using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(202)]
    public class add_indexer_flags : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blocklist").AddColumn("IndexerFlags").AsInt32().WithDefaultValue(0);
            Alter.Table("EpisodeFiles").AddColumn("IndexerFlags").AsInt32().WithDefaultValue(0);
        }
    }
}
