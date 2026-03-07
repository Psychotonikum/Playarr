using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(95)]
    public class add_additional_episodes_index : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("Episodes").OnColumn("SeriesId").Ascending()
                                              .OnColumn("SeasonNumber").Ascending()
                                              .OnColumn("EpisodeNumber").Ascending();
        }
    }
}
