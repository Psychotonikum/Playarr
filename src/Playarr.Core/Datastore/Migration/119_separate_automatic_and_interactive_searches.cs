using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(119)]
    public class separate_automatic_and_interactive_searches : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("EnableSearch").OnTable("Indexers").To("EnableAutomaticSearch");
            Alter.Table("Indexers").AddColumn("EnableInteractiveSearch").AsBoolean().Nullable();

            Execute.Sql("UPDATE \"Indexers\" SET \"EnableInteractiveSearch\" = \"EnableAutomaticSearch\"");

            Alter.Table("Indexers").AlterColumn("EnableInteractiveSearch").AsBoolean().NotNullable();
        }
    }
}
