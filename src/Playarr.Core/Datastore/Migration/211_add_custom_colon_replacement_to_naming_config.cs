using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(211)]
    public class add_custom_colon_replacement_to_naming_config : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("CustomColonReplacementFormat").AsString().WithDefaultValue("");
        }
    }
}
