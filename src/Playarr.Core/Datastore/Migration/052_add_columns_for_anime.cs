using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(52)]
    public class add_columns_for_anime : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Support XEM names
            Alter.Table("SceneMappings").AddColumn("Type").AsString().Nullable();
            Execute.Sql("DELETE FROM \"SceneMappings\"");

            // Add AnimeEpisodeFormat (set to Standard Rom format for now)
            Alter.Table("NamingConfig").AddColumn("AnimeEpisodeFormat").AsString().Nullable();
            Execute.Sql("UPDATE \"NamingConfig\" SET \"AnimeEpisodeFormat\" = \"StandardEpisodeFormat\"");
        }
    }
}
