using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(217)]
    public class add_mal_and_anilist_ids : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("MalIds").AsString().WithDefaultValue("[]");
            Alter.Table("Series").AddColumn("AniListIds").AsString().WithDefaultValue("[]");
        }
    }
}
