using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(182)]
    public class add_custom_format_score_bypass_to_delay_profile : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DelayProfiles").AddColumn("BypassIfAboveCustomFormatScore").AsBoolean().WithDefaultValue(false);
            Alter.Table("DelayProfiles").AddColumn("MinimumCustomFormatScore").AsInt32().Nullable();
        }
    }
}
