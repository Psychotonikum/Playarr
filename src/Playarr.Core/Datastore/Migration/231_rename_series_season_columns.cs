using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(231)]
    public class rename_series_season_columns : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Rename SeriesId → GameId
            Rename.Column("SeriesId").OnTable("Roms").To("GameId");
            Rename.Column("SeriesId").OnTable("RomFiles").To("GameId");
            Rename.Column("SeriesId").OnTable("History").To("GameId");
            Rename.Column("SeriesId").OnTable("Blocklist").To("GameId");
            Rename.Column("SeriesId").OnTable("PendingReleases").To("GameId");
            Rename.Column("SeriesId").OnTable("ExtraFiles").To("GameId");
            Rename.Column("SeriesId").OnTable("MetadataFiles").To("GameId");
            Rename.Column("SeriesId").OnTable("SubtitleFiles").To("GameId");
            Rename.Column("SeriesId").OnTable("DownloadHistory").To("GameId");

            // Rename SeasonNumber → PlatformNumber
            Rename.Column("SeasonNumber").OnTable("Roms").To("PlatformNumber");
            Rename.Column("SeasonNumber").OnTable("RomFiles").To("PlatformNumber");
            Rename.Column("SeasonNumber").OnTable("ExtraFiles").To("PlatformNumber");
            Rename.Column("SeasonNumber").OnTable("MetadataFiles").To("PlatformNumber");
            Rename.Column("SeasonNumber").OnTable("SubtitleFiles").To("PlatformNumber");
            Rename.Column("SeasonNumber").OnTable("SceneMappings").To("PlatformNumber");

            // Rename SceneSeasonNumber → ScenePlatformNumber
            Rename.Column("SceneSeasonNumber").OnTable("Roms").To("ScenePlatformNumber");
            Rename.Column("SceneSeasonNumber").OnTable("SceneMappings").To("ScenePlatformNumber");
        }
    }
}
