using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(150)]
    public class add_scene_mapping_origin : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings")
                .AddColumn("SceneOrigin").AsString().Nullable()
                .AddColumn("SearchMode").AsInt32().Nullable()
                .AddColumn("Comment").AsString().Nullable();
        }
    }
}
