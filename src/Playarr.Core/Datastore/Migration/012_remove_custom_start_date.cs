using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(12)]
    public class remove_custom_start_date : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("CustomStartDate").FromTable("Series");
        }
    }
}
