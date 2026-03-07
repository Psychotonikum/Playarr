using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(55)]
    public class drop_old_profile_columns : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("QualityProfileId").FromTable("Series");
        }
    }
}
