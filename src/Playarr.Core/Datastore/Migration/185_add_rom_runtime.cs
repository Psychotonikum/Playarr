using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(185)]
    public class add_episode_runtime : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("Runtime").AsInt32().WithDefaultValue(0);
        }
    }
}
