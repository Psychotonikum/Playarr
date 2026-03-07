using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(148)]
    public class mediainfo_channels : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"EpisodeFiles\" SET \"MediaInfo\" = Replace(\"MediaInfo\", '\"audioChannels\"', '\"audioChannelsContainer\"');");
            Execute.Sql("UPDATE \"EpisodeFiles\" SET \"MediaInfo\" = Replace(\"MediaInfo\", '\"audioChannelPositionsText\"', '\"audioChannelPositionsTextContainer\"');");
        }
    }
}
