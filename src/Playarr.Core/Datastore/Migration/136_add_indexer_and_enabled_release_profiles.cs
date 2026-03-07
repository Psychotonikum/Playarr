using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(136)]
    public class add_indexer_and_enabled_to_release_profiles : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ReleaseProfiles").AddColumn("Enabled").AsBoolean().WithDefaultValue(true);
            Alter.Table("ReleaseProfiles").AddColumn("IndexerId").AsInt32().WithDefaultValue(0);
        }
    }
}
