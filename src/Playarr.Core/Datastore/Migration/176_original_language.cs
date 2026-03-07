using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;
using Playarr.Core.Languages;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(176)]
    public class original_language : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English);
        }
    }
}
