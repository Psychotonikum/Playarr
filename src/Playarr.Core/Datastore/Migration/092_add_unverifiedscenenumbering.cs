using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(92)]
    public class add_unverifiedscenenumbering : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("UnverifiedSceneNumbering").AsBoolean().WithDefaultValue(false);
        }
    }
}
