using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(112)]
    public class added_regex_to_scenemapping : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings").AddColumn("FilterRegex").AsString().Nullable();
        }
    }
}
