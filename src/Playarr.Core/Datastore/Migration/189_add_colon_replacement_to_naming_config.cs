using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(189)]
    public class add_colon_replacement_to_naming_config : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("ColonReplacementFormat").AsInt32().WithDefaultValue(4);
        }
    }
}
