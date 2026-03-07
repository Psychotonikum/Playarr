using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(47)]
    public class add_temporary_blacklist_columns : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blacklist").AddColumn("PublishedDate").AsDateTime().Nullable();
        }
    }
}
