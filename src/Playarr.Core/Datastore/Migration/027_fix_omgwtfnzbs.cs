using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(27)]
    public class fix_omgwtfnzbs : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Indexers")
                  .Set(new { ConfigContract = "OmgwtfnzbsSettings" })
                  .Where(new { Implementation = "Omgwtfnzbs" });

            Update.Table("Indexers")
                  .Set(new { Settings = "{}" })
                  .Where(new { Implementation = "Omgwtfnzbs", Settings = (string)null });

            Update.Table("Indexers")
                  .Set(new { Settings = "{}" })
                  .Where(new { Implementation = "Omgwtfnzbs", Settings = "" });
        }
    }
}
