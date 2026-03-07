using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(9)]
    public class fix_rename_episodes : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("PlatformFolderFormat").FromTable("NamingConfig");

            IfDatabase("sqlite").Update.Table("NamingConfig").Set(new { RenameEpisodes = 1 }).Where(new { RenameEpisodes = -1 });
            IfDatabase("sqlite").Update.Table("NamingConfig").Set(new { RenameEpisodes = 0 }).Where(new { RenameEpisodes = -2 });
        }
    }
}
