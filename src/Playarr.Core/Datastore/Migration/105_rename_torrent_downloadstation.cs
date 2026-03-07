using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(105)]
    public class rename_torrent_downloadstation : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("DownloadClients").Set(new { Implementation = "TorrentDownloadStation" }).Where(new { Implementation = "DownloadStation" });
        }
    }
}
