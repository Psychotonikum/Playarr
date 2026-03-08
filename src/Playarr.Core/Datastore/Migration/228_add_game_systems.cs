using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(228)]
    public class add_game_systems : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("GameSystems")
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("FolderName").AsString().NotNullable()
                .WithColumn("SystemType").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("FileExtensions").AsString().Nullable()
                .WithColumn("NamingFormat").AsString().Nullable()
                .WithColumn("UpdateNamingFormat").AsString().Nullable()
                .WithColumn("DlcNamingFormat").AsString().Nullable()
                .WithColumn("BaseFolderName").AsString().Nullable()
                .WithColumn("UpdateFolderName").AsString().Nullable()
                .WithColumn("DlcFolderName").AsString().Nullable()
                .WithColumn("Tags").AsString().Nullable();

            // Add GameSystemId column to Series table
            Alter.Table("Series")
                .AddColumn("GameSystemId").AsInt32().Nullable();

            // Add RomFileType column to EpisodeFiles table
            Alter.Table("EpisodeFiles")
                .AddColumn("RomFileType").AsInt32().NotNullable().WithDefaultValue(0)
                .AddColumn("PatchVersion").AsString().Nullable()
                .AddColumn("DlcIndex").AsString().Nullable()
                .AddColumn("LinkedGameId").AsInt32().Nullable();

            // Seed default systems
            Execute.WithConnection(SeedDefaultSystems);
        }

        private void SeedDefaultSystems(IDbConnection conn, IDbTransaction tran)
        {
            var systems = new List<(string Name, string Folder, int SystemType, string Extensions, string NamingFormat, string UpdateNaming, string DlcNaming, string BaseFolder, string UpdateFolder, string DlcFolder)>
            {
                ("Super Nintendo", "snes", 0, "[\".sfc\",\".smc\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Nintendo Entertainment System", "nes", 0, "[\".nes\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Game Boy Advance", "gba", 0, "[\".gba\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Game Boy Color", "gbc", 0, "[\".gbc\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Game Boy", "gb", 0, "[\".gb\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Sega Genesis", "genesis", 0, "[\".md\",\".bin\",\".gen\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Sega Master System", "mastersystem", 0, "[\".sms\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Sega Game Gear", "gamegear", 0, "[\".gg\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Nintendo 64", "n64", 0, "[\".z64\",\".n64\",\".v64\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("PlayStation", "psx", 0, "[\".bin\",\".cue\",\".iso\",\".img\",\".pbp\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Sega Saturn", "saturn", 0, "[\".bin\",\".cue\",\".iso\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Neo Geo", "neogeo", 0, "[\".zip\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Atari 2600", "atari2600", 0, "[\".a26\",\".bin\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Atari 7800", "atari7800", 0, "[\".a78\",\".bin\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Atari Lynx", "atarilynx", 0, "[\".lnx\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("TurboGrafx-16", "tg16", 0, "[\".pce\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Sega Dreamcast", "dreamcast", 0, "[\".cdi\",\".gdi\",\".chd\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Nintendo DS", "nds", 0, "[\".nds\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("PlayStation Portable", "psp", 0, "[\".iso\",\".cso\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Arcade", "arcade", 0, "[\".zip\"]", "{Game Title} {Region}.{Extension}", "", "", "", "", ""),
                ("Nintendo Switch", "switch", 1, "[\".nsp\",\".xci\"]", "{Game Title}.{Extension}", "{Game Title} v{Version}.{Extension}", "{Game Title} DLC{Index}.{Extension}", "base", "update", "dlc"),
                ("Wii U", "wiiu", 1, "[\".wua\",\".wud\",\".wux\",\".rpx\"]", "{Game Title}.{Extension}", "{Game Title} v{Version}.{Extension}", "{Game Title} DLC{Index}.{Extension}", "base", "update", "dlc"),
                ("PlayStation 3", "ps3", 1, "[\".pkg\",\".iso\"]", "{Game Title}.{Extension}", "{Game Title} v{Version}.{Extension}", "{Game Title} DLC{Index}.{Extension}", "base", "update", "dlc"),
                ("PlayStation Vita", "psvita", 1, "[\".vpk\",\".pkg\"]", "{Game Title}.{Extension}", "{Game Title} v{Version}.{Extension}", "{Game Title} DLC{Index}.{Extension}", "base", "update", "dlc"),
                ("Nintendo 3DS", "3ds", 1, "[\".3ds\",\".cia\"]", "{Game Title}.{Extension}", "{Game Title} v{Version}.{Extension}", "{Game Title} DLC{Index}.{Extension}", "base", "update", "dlc"),
            };

            foreach (var sys in systems)
            {
                conn.Execute(
                    "INSERT INTO \"GameSystems\" (\"Name\", \"FolderName\", \"SystemType\", \"FileExtensions\", \"NamingFormat\", \"UpdateNamingFormat\", \"DlcNamingFormat\", \"BaseFolderName\", \"UpdateFolderName\", \"DlcFolderName\", \"Tags\") " +
                    "VALUES (@Name, @FolderName, @SystemType, @FileExtensions, @NamingFormat, @UpdateNamingFormat, @DlcNamingFormat, @BaseFolderName, @UpdateFolderName, @DlcFolderName, '[]')",
                    new
                    {
                        Name = sys.Name,
                        FolderName = sys.Folder,
                        SystemType = sys.SystemType,
                        FileExtensions = sys.Extensions,
                        NamingFormat = sys.NamingFormat,
                        UpdateNamingFormat = sys.UpdateNaming,
                        DlcNamingFormat = sys.DlcNaming,
                        BaseFolderName = sys.BaseFolder,
                        UpdateFolderName = sys.UpdateFolder,
                        DlcFolderName = sys.DlcFolder,
                    },
                    tran);
            }
        }
    }
}
