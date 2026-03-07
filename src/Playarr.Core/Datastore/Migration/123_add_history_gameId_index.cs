using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(123)]
    public class add_history_gameId_index : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("History").OnColumn("SeriesId");
        }
    }
}
