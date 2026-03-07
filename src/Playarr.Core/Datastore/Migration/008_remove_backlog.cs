using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(8)]
    public class remove_backlog : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("BacklogSetting").FromTable("Series");
            Delete.Column("UseSceneName").FromTable("NamingConfig");
        }
    }
}
