using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(205)]
    public class rename_season_pack_spec : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"CustomFormats\" SET \"Specifications\" = REPLACE(\"Specifications\", 'SeasonPackSpecification', 'ReleaseTypeSpecification')");
        }
    }
}
