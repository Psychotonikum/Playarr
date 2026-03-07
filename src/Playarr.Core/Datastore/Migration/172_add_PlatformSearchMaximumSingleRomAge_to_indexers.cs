using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(172)]
    public class add_SeasonSearchMaximumSingleEpisodeAge_to_indexers : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("SeasonSearchMaximumSingleEpisodeAge").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }
}
