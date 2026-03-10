using System.Collections.Generic;

namespace Playarr.Core.Games
{
    public static class DefaultGameSystems
    {
        public static List<GameSystem> GetDefaults()
        {
            return new List<GameSystem>
            {
                // Classic systems - Batocera short names
                Classic("Super Nintendo", "snes", ".sfc", ".smc"),
                Classic("Nintendo Entertainment System", "nes", ".nes"),
                Classic("Game Boy Advance", "gba", ".gba"),
                Classic("Game Boy Color", "gbc", ".gbc", ".zip"),
                Classic("Game Boy", "gb", ".gb"),
                Classic("Sega Genesis/Mega Drive", "megadrive", ".md", ".bin", ".gen"),
                Classic("Sega Master System", "mastersystem", ".sms"),
                Classic("Game Gear", "gamegear", ".gg"),
                Classic("Nintendo 64", "n64", ".z64", ".n64", ".v64"),
                Classic("Sony PlayStation", "psx", ".bin", ".cue", ".iso", ".img", ".pbp", ".chd"),
                Classic("Sega Saturn", "saturn", ".bin", ".cue", ".iso", ".chd"),
                Classic("Neo Geo", "neogeo", ".zip"),
                Classic("Atari 2600", "atari2600", ".a26", ".bin"),
                Classic("Atari 7800", "atari7800", ".a78", ".bin"),
                Classic("Atari Lynx", "lynx", ".lnx"),
                Classic("PC Engine/TurboGrafx-16", "pcengine", ".pce"),
                Classic("Sega Dreamcast", "dreamcast", ".cdi", ".gdi", ".chd"),
                Classic("Nintendo DS", "nds", ".nds"),
                Classic("PlayStation Portable", "psp", ".iso", ".cso"),
                Classic("Arcade", "mame", ".zip"),
                Classic("Sony PlayStation 2", "ps2", ".iso", ".chd", ".bin", ".cue"),
                Classic("Nintendo GameCube", "gamecube", ".iso", ".gcm", ".rvz"),
                Classic("Nintendo Wii", "wii", ".iso", ".wbfs", ".rvz"),
                Classic("Sega CD", "segacd", ".bin", ".cue", ".chd", ".iso"),
                Classic("Sega 32X", "sega32x", ".32x", ".bin"),
                Classic("Neo Geo Pocket Color", "ngpc", ".ngc"),
                Classic("WonderSwan Color", "wswanc", ".wsc"),

                // Patchable systems - Batocera short names
                Patchable("Nintendo Switch", "switch", ".nsp", ".xci", ".xcz", ".nsz"),
                Patchable("Wii U", "wiiu", ".wua", ".wud", ".wux", ".rpx"),
                Patchable("PlayStation 3", "ps3", ".pkg", ".iso"),
                Patchable("PlayStation Vita", "psvita", ".vpk", ".pkg"),
                Patchable("Nintendo 3DS", "3ds", ".3ds", ".cia"),
            };
        }

        private static GameSystem Classic(string name, string folder, params string[] extensions)
        {
            return new GameSystem
            {
                Name = name,
                FolderName = folder,
                SystemType = GameSystemType.Classic,
                FileExtensions = new List<string>(extensions),
                NamingFormat = "{Game Title} {Region}.{Extension}",
                UpdateNamingFormat = string.Empty,
                DlcNamingFormat = string.Empty,
                BaseFolderName = string.Empty,
                UpdateFolderName = string.Empty,
                DlcFolderName = string.Empty,
            };
        }

        private static GameSystem Patchable(string name, string folder, params string[] extensions)
        {
            return new GameSystem
            {
                Name = name,
                FolderName = folder,
                SystemType = GameSystemType.Patchable,
                FileExtensions = new List<string>(extensions),
                NamingFormat = "{Game Title}.{Extension}",
                UpdateNamingFormat = "{Game Title} v{Version}.{Extension}",
                DlcNamingFormat = "{Game Title} DLC{Index}.{Extension}",
                BaseFolderName = "base",
                UpdateFolderName = "update",
                DlcFolderName = "dlc",
            };
        }
    }
}
