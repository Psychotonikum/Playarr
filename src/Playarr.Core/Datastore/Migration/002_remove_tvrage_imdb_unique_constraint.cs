using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(2)]
    public class remove_tvrage_imdb_unique_constraint : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Index().OnTable("Series").OnColumn("MobyGamesId");
            Delete.Index().OnTable("Series").OnColumn("ImdbId");
        }
    }
}
