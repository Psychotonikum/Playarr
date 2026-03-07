using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(194)]
    public class add_notification_status : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("NotificationStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();
        }
    }
}
