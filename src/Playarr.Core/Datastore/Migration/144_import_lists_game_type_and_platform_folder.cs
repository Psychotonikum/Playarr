using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(144)]
    public class import_lists_series_type_and_season_folder : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("SeriesType").AsInt32().WithDefaultValue(0);
            Alter.Table("ImportLists").AddColumn("PlatformFolder").AsBoolean().WithDefaultValue(true);
        }
    }
}
