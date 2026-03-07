using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(118)]
    public class add_history_eventType_index : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("History").OnColumn("EventType");
        }
    }
}
