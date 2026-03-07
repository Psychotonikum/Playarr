using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(100)]
    public class add_scene_season_number : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings").AlterColumn("SeasonNumber").AsInt32().Nullable();
            Alter.Table("SceneMappings").AddColumn("SceneSeasonNumber").AsInt32().Nullable();
        }
    }
}
