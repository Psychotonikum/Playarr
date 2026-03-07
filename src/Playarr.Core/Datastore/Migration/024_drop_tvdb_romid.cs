using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(24)]
    public class drop_tvdb_episodeid : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("TvDbEpisodeId").FromTable("Episodes");
        }
    }
}
