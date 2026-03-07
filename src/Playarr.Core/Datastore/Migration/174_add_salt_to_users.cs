using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(174)]
    public class add_salt_to_users : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Users")
                .AddColumn("Salt").AsString().Nullable()
                .AddColumn("Iterations").AsInt32().Nullable();
        }
    }
}
