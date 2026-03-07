using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(180)]
    public class task_duration : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ScheduledTasks").AddColumn("LastStartTime").AsDateTime().Nullable();
        }
    }
}
