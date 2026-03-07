using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(58)]
    public class drop_episode_file_path : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Path").FromTable("EpisodeFiles");
        }
    }
}
