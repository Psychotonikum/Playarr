using System.Text.RegularExpressions;

namespace Playarr.Core.Games
{
    public class AerofoilParseResult
    {
        public string GameTitle { get; set; }
        public string Region { get; set; }
        public string Extension { get; set; }
        public RomFileType FileType { get; set; }
        public string Version { get; set; }
        public string DlcIndex { get; set; }
    }

    public static class AerofoilFileNameParser
    {
        // Matches: "Game Title v12345.ext" (update)
        private static readonly Regex UpdateRegex = new Regex(
            @"^(?<title>.+?)\s+v(?<version>\d+)\.(?<ext>[^.]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches: "Game Title DLC01.ext" (dlc)
        private static readonly Regex DlcRegex = new Regex(
            @"^(?<title>.+?)\s+DLC(?<index>\d+)\.(?<ext>[^.]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches: "Game Title Region.ext" (classic with region)
        private static readonly Regex ClassicWithRegionRegex = new Regex(
            @"^(?<title>.+?)\s+(?<region>USA|EUR|JPN|JAP|KOR|CHN|TWN|AUS|BRA|FRA|GER|ITA|SPA|World)\.(?<ext>[^.]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches: "Game Title.ext" (base game, no region)
        private static readonly Regex BaseRegex = new Regex(
            @"^(?<title>.+?)\.(?<ext>[^.]+)$",
            RegexOptions.Compiled);

        public static AerofoilParseResult Parse(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            // Try update pattern first: "Game Title v12345.ext"
            var match = UpdateRegex.Match(fileName);
            if (match.Success)
            {
                return new AerofoilParseResult
                {
                    GameTitle = match.Groups["title"].Value.Trim(),
                    FileType = RomFileType.Update,
                    Version = match.Groups["version"].Value,
                    Extension = match.Groups["ext"].Value
                };
            }

            // Try DLC pattern: "Game Title DLC01.ext"
            match = DlcRegex.Match(fileName);
            if (match.Success)
            {
                return new AerofoilParseResult
                {
                    GameTitle = match.Groups["title"].Value.Trim(),
                    FileType = RomFileType.Dlc,
                    DlcIndex = match.Groups["index"].Value,
                    Extension = match.Groups["ext"].Value
                };
            }

            // Try classic with region: "Game Title USA.ext"
            match = ClassicWithRegionRegex.Match(fileName);
            if (match.Success)
            {
                return new AerofoilParseResult
                {
                    GameTitle = match.Groups["title"].Value.Trim(),
                    Region = match.Groups["region"].Value,
                    FileType = RomFileType.Base,
                    Extension = match.Groups["ext"].Value
                };
            }

            // Fallback to base game: "Game Title.ext"
            match = BaseRegex.Match(fileName);
            if (match.Success)
            {
                return new AerofoilParseResult
                {
                    GameTitle = match.Groups["title"].Value.Trim(),
                    FileType = RomFileType.Base,
                    Extension = match.Groups["ext"].Value
                };
            }

            return null;
        }

        /// <summary>
        /// Extract the base game title from a filename, stripping version/DLC/region suffixes.
        /// Used to link updates and DLC back to their base game.
        /// </summary>
        public static string ExtractGameTitle(string fileName)
        {
            var result = Parse(fileName);
            return result?.GameTitle;
        }
    }
}
