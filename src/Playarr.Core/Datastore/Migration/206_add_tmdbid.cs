using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(206)]
    public class add_tmdbid : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("TmdbId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("TmdbId");
        }
    }
}
