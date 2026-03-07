using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(124)]
    public class remove_media_browser_metadata : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Metadata").Row(new { Implementation = "MediaBrowserMetadata" });
            Delete.FromTable("MetadataFiles").Row(new { Consumer = "MediaBrowserMetadata" });
        }
    }
}
