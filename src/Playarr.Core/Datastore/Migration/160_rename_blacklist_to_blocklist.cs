using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(160)]
    public class rename_blacklist_to_blocklist : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Blacklist").To("Blocklist");
        }
    }
}
