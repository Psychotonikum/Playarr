using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(137)]
    public class add_airedbefore_to_episodes : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AiredAfterPlatformNumber").AsInt32().Nullable()
                                   .AddColumn("AiredBeforePlatformNumber").AsInt32().Nullable()
                                   .AddColumn("AiredBeforeRomNumber").AsInt32().Nullable();
        }
    }
}
