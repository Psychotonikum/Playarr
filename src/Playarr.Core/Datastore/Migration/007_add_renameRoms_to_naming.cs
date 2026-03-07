using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(7)]
    public class add_renameRoms_to_naming : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig")
                .AddColumn("RenameEpisodes")
                .AsBoolean()
                .Nullable();

            Execute.Sql("UPDATE \"NamingConfig\" SET \"RenameEpisodes\" = NOT \"UseSceneName\"");
        }
    }
}
