using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(151)]
    public class remove_custom_filter_type : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("CustomFilters").Set(new { Type = "game" }).Where(new { Type = "seriesIndex" });
            Update.Table("CustomFilters").Set(new { Type = "game" }).Where(new { Type = "seriesEditor" });
            Update.Table("CustomFilters").Set(new { Type = "game" }).Where(new { Type = "platformPass" });
        }
    }
}
