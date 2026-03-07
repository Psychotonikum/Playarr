using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(154)]
    public class add_name_release_profile : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ReleaseProfiles").AddColumn("Name").AsString().Nullable().WithDefaultValue(null);
        }
    }
}
