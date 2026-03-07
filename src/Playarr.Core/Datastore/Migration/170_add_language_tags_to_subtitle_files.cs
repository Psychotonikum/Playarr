using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(170)]
    public class add_language_tags_to_subtitle_files : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SubtitleFiles").AddColumn("LanguageTags").AsString().Nullable();
        }
    }
}
