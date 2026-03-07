using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(5)]
    public class added_eventtype_to_history : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("History")
                .AddColumn("EventType")
                .AsInt32()
                .Nullable();
        }
    }
}
