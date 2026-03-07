using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(22)]
    public class move_indexer_to_generic_provider : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("ConfigContract").AsString().Nullable();
        }
    }
}
