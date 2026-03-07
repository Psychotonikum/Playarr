using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(200)]
    public class AddNewItemMonitorType : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("MonitorNewItems").AsInt32().WithDefaultValue(0);
            Alter.Table("ImportLists").AddColumn("MonitorNewItems").AsInt32().WithDefaultValue(0);
        }
    }
}
