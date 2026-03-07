using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(91)]
    public class added_indexerstatus : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("IndexerStatus")
                  .WithColumn("IndexerId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTime().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTime().Nullable()
                  .WithColumn("LastRssSyncReleaseInfo").AsString().Nullable();
        }
    }
}
