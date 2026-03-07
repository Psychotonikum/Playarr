using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(13)]
    public class add_air_date_utc : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AirDateUtc").AsDateTime().Nullable();

            Execute.Sql("UPDATE \"Episodes\" SET \"AirDateUtc\" = \"AirDate\"");
        }
    }
}
