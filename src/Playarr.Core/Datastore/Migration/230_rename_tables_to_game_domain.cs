using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(230)]
    public class rename_tables_to_game_domain : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Series").To("Games");
            Rename.Table("Episodes").To("Roms");
            Rename.Table("EpisodeFiles").To("RomFiles");
        }
    }
}
