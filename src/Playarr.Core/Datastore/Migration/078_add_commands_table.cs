using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(78)]
    public class add_commands_table : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Commands")
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Body").AsString().NotNullable()
                  .WithColumn("Priority").AsInt32().NotNullable()
                  .WithColumn("Status").AsInt32().NotNullable()
                  .WithColumn("QueuedAt").AsDateTime().NotNullable()
                  .WithColumn("StartedAt").AsDateTime().Nullable()
                  .WithColumn("EndedAt").AsDateTime().Nullable()
                  .WithColumn("Duration").AsString().Nullable()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("Trigger").AsInt32().NotNullable();
        }
    }
}
