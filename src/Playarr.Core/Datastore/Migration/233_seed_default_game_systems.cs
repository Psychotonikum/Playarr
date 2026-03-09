using System.Data;
using Dapper;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(233)]
    public class seed_default_game_systems : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(SeedGameSystems);
        }

        private void SeedGameSystems(IDbConnection conn, IDbTransaction tran)
        {
            var count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM \"GameSystems\"", transaction: tran);

            if (count > 0)
            {
                return;
            }

            var systems = new[]
            {
                // 3rd Generation (8-bit)
                new { Name = "Nintendo Entertainment System", FolderName = "nes", SystemType = 0, FileExtensions = "[\".nes\",\".unf\",\".unif\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Sega Master System", FolderName = "mastersystem", SystemType = 0, FileExtensions = "[\".sms\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },

                // 4th Generation (16-bit)
                new { Name = "Super Nintendo", FolderName = "snes", SystemType = 0, FileExtensions = "[\".sfc\",\".smc\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Sega Genesis / Mega Drive", FolderName = "megadrive", SystemType = 0, FileExtensions = "[\".md\",\".gen\",\".bin\",\".smd\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Game Boy", FolderName = "gb", SystemType = 0, FileExtensions = "[\".gb\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },

                // 5th Generation (32/64-bit)
                new { Name = "Sony PlayStation", FolderName = "psx", SystemType = 0, FileExtensions = "[\".bin\",\".cue\",\".iso\",\".chd\",\".pbp\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Nintendo 64", FolderName = "n64", SystemType = 0, FileExtensions = "[\".z64\",\".n64\",\".v64\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Sega Saturn", FolderName = "saturn", SystemType = 0, FileExtensions = "[\".bin\",\".cue\",\".iso\",\".chd\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Game Boy Advance", FolderName = "gba", SystemType = 0, FileExtensions = "[\".gba\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },

                // 6th Generation
                new { Name = "Sony PlayStation 2", FolderName = "ps2", SystemType = 0, FileExtensions = "[\".iso\",\".bin\",\".chd\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Nintendo GameCube", FolderName = "gc", SystemType = 0, FileExtensions = "[\".iso\",\".gcz\",\".rvz\",\".nkit.iso\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Sega Dreamcast", FolderName = "dreamcast", SystemType = 0, FileExtensions = "[\".gdi\",\".cdi\",\".chd\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },

                // 7th Generation
                new { Name = "Nintendo Wii", FolderName = "wii", SystemType = 0, FileExtensions = "[\".iso\",\".wbfs\",\".rvz\",\".nkit.iso\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Sony PlayStation 3", FolderName = "ps3", SystemType = 1, FileExtensions = "[\".iso\",\".pkg\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "{Game Title} v{Version}.{Extension}", DlcNamingFormat = "{Game Title} DLC{Index}.{Extension}", BaseFolderName = "base", UpdateFolderName = "update", DlcFolderName = "dlc", Tags = "[]" },
                new { Name = "Sony PlayStation Portable", FolderName = "psp", SystemType = 0, FileExtensions = "[\".iso\",\".cso\",\".pbp\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "Nintendo DS", FolderName = "nds", SystemType = 0, FileExtensions = "[\".nds\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },

                // 8th Generation
                new { Name = "Nintendo Wii U", FolderName = "wiiu", SystemType = 1, FileExtensions = "[\".wua\",\".wud\",\".wux\",\".rpx\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "{Game Title} v{Version}.{Extension}", DlcNamingFormat = "{Game Title} DLC{Index}.{Extension}", BaseFolderName = "base", UpdateFolderName = "update", DlcFolderName = "dlc", Tags = "[]" },
                new { Name = "Nintendo 3DS", FolderName = "3ds", SystemType = 1, FileExtensions = "[\".3ds\",\".cia\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "{Game Title} v{Version}.{Extension}", DlcNamingFormat = "{Game Title} DLC{Index}.{Extension}", BaseFolderName = "base", UpdateFolderName = "update", DlcFolderName = "dlc", Tags = "[]" },
                new { Name = "Sony PlayStation Vita", FolderName = "psvita", SystemType = 1, FileExtensions = "[\".vpk\",\".mai\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "{Game Title} v{Version}.{Extension}", DlcNamingFormat = "{Game Title} DLC{Index}.{Extension}", BaseFolderName = "base", UpdateFolderName = "update", DlcFolderName = "dlc", Tags = "[]" },

                // 9th Generation / Current
                new { Name = "Nintendo Switch", FolderName = "switch", SystemType = 1, FileExtensions = "[\".nsp\",\".nsz\",\".xci\",\".xcz\"]", NamingFormat = "{Game Title} {Region}.{Extension}", UpdateNamingFormat = "{Game Title} v{Version}.{Extension}", DlcNamingFormat = "{Game Title} DLC{Index}.{Extension}", BaseFolderName = "base", UpdateFolderName = "update", DlcFolderName = "dlc", Tags = "[]" },

                // PC
                new { Name = "DOS", FolderName = "dos", SystemType = 0, FileExtensions = "[\".exe\",\".com\",\".bat\"]", NamingFormat = "{Game Title}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
                new { Name = "ScummVM", FolderName = "scummvm", SystemType = 0, FileExtensions = "[\".scummvm\"]", NamingFormat = "{Game Title}.{Extension}", UpdateNamingFormat = "", DlcNamingFormat = "", BaseFolderName = "", UpdateFolderName = "", DlcFolderName = "", Tags = "[]" },
            };

            foreach (var system in systems)
            {
                conn.Execute(
                    "INSERT INTO \"GameSystems\" (\"Name\", \"FolderName\", \"SystemType\", \"FileExtensions\", \"NamingFormat\", \"UpdateNamingFormat\", \"DlcNamingFormat\", \"BaseFolderName\", \"UpdateFolderName\", \"DlcFolderName\", \"Tags\") VALUES (@Name, @FolderName, @SystemType, @FileExtensions, @NamingFormat, @UpdateNamingFormat, @DlcNamingFormat, @BaseFolderName, @UpdateFolderName, @DlcFolderName, @Tags)",
                    system,
                    tran);
            }
        }
    }
}
