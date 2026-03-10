using System;
using System.Collections.Generic;
using Playarr.Core.Qualities;

namespace Playarr.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static Dictionary<string, Quality> _fileExtensions;

        static MediaFileExtensions()
        {
            _fileExtensions = new Dictionary<string, Quality>(StringComparer.OrdinalIgnoreCase)
            {
                // Nintendo
                { ".nes", Quality.Unknown },
                { ".smc", Quality.Unknown },
                { ".sfc", Quality.Unknown },
                { ".gb", Quality.Unknown },
                { ".gbc", Quality.Unknown },
                { ".gba", Quality.Unknown },
                { ".n64", Quality.Unknown },
                { ".z64", Quality.Unknown },
                { ".v64", Quality.Unknown },
                { ".nds", Quality.Unknown },
                { ".3ds", Quality.Unknown },
                { ".cia", Quality.Unknown },
                { ".nsp", Quality.Unknown },
                { ".xci", Quality.Unknown },
                { ".xcz", Quality.Unknown },
                { ".nsz", Quality.Unknown },
                { ".nro", Quality.Unknown },
                { ".wad", Quality.Unknown },
                { ".wbfs", Quality.Unknown },
                { ".wia", Quality.Unknown },
                { ".rvz", Quality.Unknown },
                { ".gcm", Quality.Unknown },
                { ".gcz", Quality.Unknown },
                { ".dol", Quality.Unknown },
                { ".wua", Quality.Unknown },
                { ".wud", Quality.Unknown },
                { ".wux", Quality.Unknown },
                { ".rpx", Quality.Unknown },

                // Sony
                { ".pbp", Quality.Unknown },
                { ".pkg", Quality.Unknown },
                { ".vpk", Quality.Unknown },

                // Sega
                { ".sms", Quality.Unknown },
                { ".gg", Quality.Unknown },
                { ".md", Quality.Unknown },
                { ".gen", Quality.Unknown },
                { ".32x", Quality.Unknown },
                { ".cdi", Quality.Unknown },
                { ".gdi", Quality.Unknown },
                { ".cue", Quality.Unknown },

                // Atari
                { ".a26", Quality.Unknown },
                { ".a52", Quality.Unknown },
                { ".a78", Quality.Unknown },
                { ".lnx", Quality.Unknown },
                { ".jag", Quality.Unknown },

                // NEC / SNK / Bandai
                { ".pce", Quality.Unknown },
                { ".ngc", Quality.Unknown },
                { ".wsc", Quality.Unknown },

                // General ROM/Disc Formats
                { ".iso", Quality.Unknown },
                { ".bin", Quality.Unknown },
                { ".img", Quality.Unknown },
                { ".chd", Quality.Unknown },
                { ".cso", Quality.Unknown },
                { ".ecm", Quality.Unknown },
                { ".mdf", Quality.Unknown },
                { ".mds", Quality.Unknown },
                { ".7z", Quality.Unknown },
                { ".zip", Quality.Unknown },
                { ".rar", Quality.Unknown }
            };
        }

        public static HashSet<string> Extensions => new HashSet<string>(_fileExtensions.Keys, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> DiskExtensions => new HashSet<string>(new[] { ".img", ".iso", ".chd", ".cso", ".mdf", ".gdi", ".cdi", ".cue", ".wbfs", ".wia", ".rvz", ".gcm", ".gcz", ".wud", ".wux" }, StringComparer.OrdinalIgnoreCase);

        // ROM extensions that should not be analyzed by ffprobe
        public static HashSet<string> RomExtensions => new HashSet<string>(
            new[]
            {
                ".nes", ".smc", ".sfc", ".gb", ".gbc", ".gba", ".n64", ".z64", ".v64",
                ".nds", ".3ds", ".cia", ".nsp", ".xci", ".xcz", ".nsz", ".nro", ".wad", ".wbfs", ".wia",
                ".rvz", ".gcm", ".gcz", ".dol", ".wua", ".wud", ".wux", ".rpx", ".pbp", ".pkg", ".vpk",
                ".sms", ".gg", ".md", ".gen", ".32x", ".cdi", ".gdi", ".cue",
                ".a26", ".a52", ".a78", ".lnx", ".jag",
                ".pce", ".ngc", ".wsc",
                ".img", ".iso", ".bin", ".chd", ".cso", ".ecm", ".mdf", ".mds",
                ".7z", ".zip", ".rar"
            },
            StringComparer.OrdinalIgnoreCase);

        public static Quality GetQualityForExtension(string extension)
        {
            if (_fileExtensions.ContainsKey(extension))
            {
                return _fileExtensions[extension];
            }

            return Quality.Unknown;
        }
    }
}
