using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(193)]
    public class add_import_list_items : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("ImportListItems")
                .WithColumn("ImportListId").AsInt32()
                .WithColumn("Title").AsString()
                .WithColumn("TvdbId").AsInt32()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("TmdbId").AsInt32().Nullable()
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("MalId").AsInt32().Nullable()
                .WithColumn("AniListId").AsInt32().Nullable()
                .WithColumn("ReleaseDate").AsDateTimeOffset().Nullable();

            Alter.Table("ImportListStatus")
                .AddColumn("HasRemovedItemSinceLastClean").AsBoolean().WithDefaultValue(false);
        }
    }
}
