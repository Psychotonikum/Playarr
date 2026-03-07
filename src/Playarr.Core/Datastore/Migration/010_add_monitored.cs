using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(10)]
    public class add_monitored : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("Monitored").AsBoolean().Nullable();
            Alter.Table("Platforms").AddColumn("Monitored").AsBoolean().Nullable();

            Update.Table("Episodes").Set(new { Monitored = true }).Where(new { Ignored = false });
            Update.Table("Episodes").Set(new { Monitored = false }).Where(new { Ignored = true });

            Update.Table("Platforms").Set(new { Monitored = true }).Where(new { Ignored = false });
            Update.Table("Platforms").Set(new { Monitored = false }).Where(new { Ignored = true });
        }
    }
}
