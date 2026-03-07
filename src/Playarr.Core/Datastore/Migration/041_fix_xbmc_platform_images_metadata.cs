using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(41)]
    public class fix_xbmc_season_images_metadata : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"MetadataFiles\" SET \"Type\" = 4 WHERE \"Consumer\" = 'XbmcMetadata' AND \"SeasonNumber\" IS NOT NULL");
        }
    }
}
