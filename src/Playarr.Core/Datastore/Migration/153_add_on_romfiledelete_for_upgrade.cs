using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(153)]
    public class add_on_episodefiledelete_for_upgrade : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnRomFileDeleteForUpgrade").AsBoolean().WithDefaultValue(true);
        }
    }
}
