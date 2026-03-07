using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(76)]
    public class add_users_table : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Users")
                  .WithColumn("Identifier").AsString().NotNullable().Unique()
                  .WithColumn("Username").AsString().NotNullable().Unique()
                  .WithColumn("Password").AsString().NotNullable();
        }
    }
}
