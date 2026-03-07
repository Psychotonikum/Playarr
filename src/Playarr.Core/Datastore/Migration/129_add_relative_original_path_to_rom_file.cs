using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(129)]
    public class add_relative_original_path_to_episode_file : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles").AddColumn("OriginalFilePath").AsString().Nullable();
        }
    }
}
