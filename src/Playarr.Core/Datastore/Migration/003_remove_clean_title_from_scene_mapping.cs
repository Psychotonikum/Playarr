using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(3)]
    public class remove_renamed_scene_mapping_columns : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("SceneMappings");

            Create.TableForModel("SceneMappings")
                  .WithColumn("TvdbId").AsInt32()
                  .WithColumn("SeasonNumber").AsInt32()
                  .WithColumn("SearchTerm").AsString()
                  .WithColumn("ParseTerm").AsString();
        }
    }
}
