using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(229)]
    public class rename_tvdbid_to_igdbid : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("TvdbId").OnTable("Series").To("IgdbId");
            Rename.Column("TvdbId").OnTable("Episodes").To("IgdbId");
            Rename.Column("TvdbId").OnTable("SceneMappings").To("IgdbId");
            Rename.Column("TvdbId").OnTable("ImportListExclusions").To("IgdbId");
            Rename.Column("TvdbId").OnTable("ImportListItems").To("IgdbId");
        }
    }
}
