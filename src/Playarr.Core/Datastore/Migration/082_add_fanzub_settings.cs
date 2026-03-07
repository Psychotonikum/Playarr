using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(82)]
    public class add_fanzub_settings : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Indexers").Set(new { ConfigContract = "FanzubSettings" }).Where(new { Implementation = "Fanzub", ConfigContract = "NullConfig" });
        }
    }
}
