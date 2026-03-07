using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(34)]
    public class remove_series_contraints : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AlterColumn("MobyGamesId").AsInt32()
                .AlterColumn("ImdbId").AsString().Nullable()
                .AlterColumn("TitleSlug").AsString().Nullable();
        }
    }
}
