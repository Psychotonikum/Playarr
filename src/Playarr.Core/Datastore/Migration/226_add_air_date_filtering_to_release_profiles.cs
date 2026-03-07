using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration;

[Migration(226)]
public class add_air_date_filtering_to_release_profiles : PlayarrMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("ReleaseProfiles").AddColumn("AirDateRestriction").AsBoolean().WithDefaultValue(false);
        Alter.Table("ReleaseProfiles").AddColumn("AirDateGracePeriod").AsInt32().WithDefaultValue(0);
    }
}
