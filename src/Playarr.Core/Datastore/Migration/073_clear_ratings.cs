using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(73)]
    public class clear_ratings : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Series")
                  .Set(new { Ratings = "{}" })
                  .AllRows();

            Update.Table("Episodes")
                  .Set(new { Ratings = "{}" })
                  .AllRows();
        }
    }
}
