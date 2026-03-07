using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(183)]
    public class update_images_remote_url : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Episodes\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"')");
            Execute.Sql("UPDATE \"Series\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"'), \"Actors\" = REPLACE(\"Actors\", '\"url\"', '\"remoteUrl\"'), \"Platforms\" = REPLACE(\"Platforms\", '\"url\"', '\"remoteUrl\"')");
        }
    }
}
