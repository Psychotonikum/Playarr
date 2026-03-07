using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(227)]
    public class original_country : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("OriginalCountry").AsString().Nullable();
        }
    }
}
