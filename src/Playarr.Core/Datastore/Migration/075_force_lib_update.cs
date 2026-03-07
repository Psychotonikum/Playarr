using System;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(75)]
    public class force_lib_update : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("ScheduledTasks")
                .Set(new { LastExecution = "2014-01-01 00:00:00" })
                .Where(new { TypeName = "Playarr.Core.Games.Commands.RefreshSeriesCommand" });

            Update.Table("Series")
                .Set(new { LastInfoSync = "2014-01-01 00:00:00" })
                .AllRows();
        }
    }

    public class ScheduledTasks75
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public int Interval { get; set; }
        public DateTime LastExecution { get; set; }
    }
}
