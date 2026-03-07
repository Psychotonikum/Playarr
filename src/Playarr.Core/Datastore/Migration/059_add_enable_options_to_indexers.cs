using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(59)]
    public class add_enable_options_to_indexers : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers")
                 .AddColumn("EnableRss").AsBoolean().Nullable()
                 .AddColumn("EnableSearch").AsBoolean().Nullable();

            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = \"Enable\", \"EnableSearch\" = \"Enable\"");
            Update.Table("Indexers").Set(new { EnableSearch = false }).Where(new { Implementation = "Wombles" });
        }
    }
}
