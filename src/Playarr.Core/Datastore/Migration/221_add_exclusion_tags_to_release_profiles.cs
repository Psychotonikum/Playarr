using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(221)]
    public class add_exclusion_tags_to_release_profiles : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ReleaseProfiles").AddColumn("ExcludedTags").AsString().NotNullable().WithDefaultValue("[]");
        }
    }
}
