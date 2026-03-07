using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(6)]
    public class add_index_to_log_time : PlayarrMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Delete.Table("Logs");

            Create.TableForModel("Logs")
                  .WithColumn("Message").AsString()
                  .WithColumn("Time").AsDateTime().Indexed()
                  .WithColumn("Logger").AsString()
                  .WithColumn("Method").AsString().Nullable()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString();
        }
    }
}
