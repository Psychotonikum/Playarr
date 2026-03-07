using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(56)]
    public class add_mediainfo_to_episodefile : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles").AddColumn("MediaInfo").AsString().Nullable();
        }
    }
}
