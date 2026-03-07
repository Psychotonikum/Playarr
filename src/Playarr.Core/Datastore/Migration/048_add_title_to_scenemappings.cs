using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(48)]
    public class add_title_to_scenemappings : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings").AddColumn("Title").AsString().Nullable();
        }
    }
}
