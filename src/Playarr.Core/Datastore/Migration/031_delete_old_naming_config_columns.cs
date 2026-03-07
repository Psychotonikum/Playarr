using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(31)]
    public class delete_old_naming_config_columns : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Separator")
                  .Column("NumberStyle")
                  .Column("IncludeGameTitle")
                  .Column("IncludeRomTitle")
                  .Column("IncludeQuality")
                  .Column("ReplaceSpaces")
                  .FromTable("NamingConfig");
        }
    }
}
