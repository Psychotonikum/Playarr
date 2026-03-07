using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(14)]
    public class drop_air_date : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("AirDate").FromTable("Episodes");
        }
    }
}
