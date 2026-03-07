using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(141)]
    public class add_update_history : PlayarrMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Create.TableForModel("UpdateHistory")
                  .WithColumn("Date").AsDateTime().NotNullable().Indexed()
                  .WithColumn("Version").AsString().NotNullable()
                  .WithColumn("EventType").AsInt32().NotNullable();
        }
    }
}
