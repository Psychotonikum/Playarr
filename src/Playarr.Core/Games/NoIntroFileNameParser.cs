using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Playarr.Core.Languages;

namespace Playarr.Core.Games
{
    public class NoIntroParseResult
    {
        public string Title { get; set; }
        public RomRegion Region { get; set; } = RomRegion.Unknown;
        public List<Language> Languages { get; set; } = new();
        public string Revision { get; set; }
        public RomReleaseType ReleaseType { get; set; } = RomReleaseType.Retail;
        public RomModificationType Modification { get; set; } = RomModificationType.Original;
        public string ModificationName { get; set; }
        public DumpQuality DumpQuality { get; set; } = DumpQuality.Unknown;
        public string Extension { get; set; }
        public RomFileType FileType { get; set; } = RomFileType.Base;
        public string PatchVersion { get; set; }
        public string DlcName { get; set; }
    }

    /// <summary>
    /// Parser for No-Intro standard ROM filenames.
    /// Format: Title (Region) (Language) (Revision) (Release Type) (Hack) [Quality] [Tags].ext
    /// </summary>
    public static class NoIntroFileNameParser
    {
        // Extract parenthesized and bracketed tags from the filename
        private static readonly Regex ParenTagRegex = new Regex(
            @"\(([^)]+)\)",
            RegexOptions.Compiled);

        private static readonly Regex BracketTagRegex = new Regex(
            @"\[([^\]]+)\]",
            RegexOptions.Compiled);

        // Region mappings
        private static readonly Dictionary<string, RomRegion> RegionMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "USA", RomRegion.USA },
            { "U", RomRegion.USA },
            { "US", RomRegion.USA },
            { "NTSC", RomRegion.USA },
            { "NTSC-U", RomRegion.USA },
            { "Europe", RomRegion.Europe },
            { "E", RomRegion.Europe },
            { "EUR", RomRegion.Europe },
            { "PAL", RomRegion.Europe },
            { "NTSC-E", RomRegion.Europe },
            { "Japan", RomRegion.Japan },
            { "J", RomRegion.Japan },
            { "JPN", RomRegion.Japan },
            { "JAP", RomRegion.Japan },
            { "NTSC-J", RomRegion.Japan },
            { "World", RomRegion.World },
            { "W", RomRegion.World },
            { "Asia", RomRegion.Asia },
            { "A", RomRegion.Asia },
            { "Australia", RomRegion.Australia },
            { "AUS", RomRegion.Australia },
            { "Korea", RomRegion.Korea },
            { "K", RomRegion.Korea },
            { "KOR", RomRegion.Korea },
            { "Brazil", RomRegion.Brazil },
            { "BR", RomRegion.Brazil },
            { "BRA", RomRegion.Brazil },
            { "France", RomRegion.Europe },
            { "FRA", RomRegion.Europe },
            { "Germany", RomRegion.Europe },
            { "GER", RomRegion.Europe },
            { "Italy", RomRegion.Europe },
            { "ITA", RomRegion.Europe },
            { "Spain", RomRegion.Europe },
            { "SPA", RomRegion.Europe },
            { "China", RomRegion.Asia },
            { "CHN", RomRegion.Asia },
            { "Taiwan", RomRegion.Asia },
            { "TWN", RomRegion.Asia },
        };

        // Language code mappings (No-Intro uses En, Fr, De, etc.)
        private static readonly Dictionary<string, Language> LanguageMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "En", Language.English },
            { "Eng", Language.English },
            { "Fr", Language.French },
            { "Fre", Language.French },
            { "De", Language.German },
            { "Ger", Language.German },
            { "Es", Language.Spanish },
            { "Spa", Language.Spanish },
            { "It", Language.Italian },
            { "Ita", Language.Italian },
            { "Ja", Language.Japanese },
            { "Jpn", Language.Japanese },
            { "Pt", Language.Portuguese },
            { "Por", Language.Portuguese },
            { "Ru", Language.Russian },
            { "Rus", Language.Russian },
            { "Zh", Language.Chinese },
            { "Chi", Language.Chinese },
            { "Ko", Language.Korean },
            { "Kor", Language.Korean },
            { "Nl", Language.Dutch },
            { "Dut", Language.Dutch },
            { "Sv", Language.Swedish },
            { "Swe", Language.Swedish },
            { "No", Language.Norwegian },
            { "Nor", Language.Norwegian },
            { "Da", Language.Danish },
            { "Dan", Language.Danish },
            { "Fi", Language.Finnish },
            { "Fin", Language.Finnish },
            { "Pl", Language.Polish },
            { "Pol", Language.Polish },
            { "Tr", Language.Turkish },
            { "Tur", Language.Turkish },
        };

        // Revision patterns
        private static readonly Regex RevisionRegex = new Regex(
            @"^Rev\s*([A-Za-z0-9.]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex VersionRegex = new Regex(
            @"^v(\d+[\d.]*\d*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Update pattern for patchable systems
        private static readonly Regex UpdateRegex = new Regex(
            @"^Update\s*v?([\d.]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // DLC pattern
        private static readonly Regex DlcRegex = new Regex(
            @"^DLC\s*(.*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static NoIntroParseResult Parse(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var result = new NoIntroParseResult();

            // Separate extension
            var ext = Path.GetExtension(fileName);
            result.Extension = ext?.TrimStart('.');
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            // Extract all parenthesized tags
            var parenMatches = ParenTagRegex.Matches(nameWithoutExt);
            var bracketMatches = BracketTagRegex.Matches(nameWithoutExt);

            // Title is everything before the first tag
            var firstTagIndex = int.MaxValue;
            if (parenMatches.Count > 0)
            {
                firstTagIndex = Math.Min(firstTagIndex, parenMatches[0].Index);
            }

            if (bracketMatches.Count > 0)
            {
                firstTagIndex = Math.Min(firstTagIndex, bracketMatches[0].Index);
            }

            result.Title = firstTagIndex < int.MaxValue
                ? nameWithoutExt.Substring(0, firstTagIndex).Trim()
                : nameWithoutExt.Trim();

            // Process parenthesized tags
            foreach (Match match in parenMatches)
            {
                var tag = match.Groups[1].Value.Trim();
                ProcessParenTag(tag, result);
            }

            // Process bracketed tags
            foreach (Match match in bracketMatches)
            {
                var tag = match.Groups[1].Value.Trim();
                ProcessBracketTag(tag, result);
            }

            // Infer languages from region if none explicitly set
            if (result.Languages.Count == 0)
            {
                result.Languages = InferLanguagesFromRegion(result.Region);
            }

            return result;
        }

        private static void ProcessParenTag(string tag, NoIntroParseResult result)
        {
            // Check for multi-region: (USA, Europe) or (USA, Japan)
            if (tag.Contains(','))
            {
                var parts = tag.Split(',').Select(p => p.Trim()).ToArray();

                // Check if all parts are regions
                if (parts.All(p => RegionMap.ContainsKey(p)))
                {
                    result.Region = RegionMap[parts[0]];
                    return;
                }

                // Check if all parts are languages: (En,Fr,De)
                if (parts.All(p => LanguageMap.ContainsKey(p)))
                {
                    foreach (var part in parts)
                    {
                        if (LanguageMap.TryGetValue(part, out var lang) && !result.Languages.Contains(lang))
                        {
                            result.Languages.Add(lang);
                        }
                    }

                    return;
                }
            }

            // Single region
            if (RegionMap.TryGetValue(tag, out var region))
            {
                result.Region = region;
                return;
            }

            // Revision: (Rev 1), (Rev A)
            var revMatch = RevisionRegex.Match(tag);
            if (revMatch.Success)
            {
                result.Revision = revMatch.Groups[1].Value;
                return;
            }

            // Version: (v1.1), (v1.2)
            var verMatch = VersionRegex.Match(tag);
            if (verMatch.Success)
            {
                result.Revision = "v" + verMatch.Groups[1].Value;
                return;
            }

            // Update: (Update v1.2.0)
            var updateMatch = UpdateRegex.Match(tag);
            if (updateMatch.Success)
            {
                result.ReleaseType = RomReleaseType.Update;
                result.FileType = RomFileType.Update;
                result.PatchVersion = updateMatch.Groups[1].Value;
                return;
            }

            // DLC: (DLC Expansion Pass)
            var dlcMatch = DlcRegex.Match(tag);
            if (dlcMatch.Success)
            {
                result.ReleaseType = RomReleaseType.Dlc;
                result.FileType = RomFileType.Dlc;
                result.DlcName = dlcMatch.Groups[1].Value.Trim();
                return;
            }

            // Release types
            switch (tag.ToLowerInvariant())
            {
                case "beta":
                    result.ReleaseType = RomReleaseType.Beta;
                    return;
                case "proto":
                case "prototype":
                    result.ReleaseType = RomReleaseType.Prototype;
                    return;
                case "demo":
                    result.ReleaseType = RomReleaseType.Demo;
                    return;
                case "sample":
                    result.ReleaseType = RomReleaseType.Sample;
                    return;
                case "promo":
                    result.ReleaseType = RomReleaseType.Promo;
                    return;
                case "competition":
                    result.ReleaseType = RomReleaseType.Competition;
                    return;
            }

            // Modification types
            switch (tag.ToLowerInvariant())
            {
                case "hack":
                    result.Modification = RomModificationType.Hack;
                    return;
                case "homebrew":
                    result.Modification = RomModificationType.Homebrew;
                    return;
                case "unl":
                case "unlicensed":
                    result.Modification = RomModificationType.Unlicensed;
                    return;
                case "pirate":
                    result.Modification = RomModificationType.Pirate;
                    return;
            }

            // Hack name: if previous tag was Hack, this is the name
            if (result.Modification == RomModificationType.Hack && string.IsNullOrEmpty(result.ModificationName))
            {
                result.ModificationName = tag;
            }
        }

        private static void ProcessBracketTag(string tag, NoIntroParseResult result)
        {
            switch (tag.ToLowerInvariant())
            {
                case "!":
                    result.DumpQuality = DumpQuality.Verified;
                    return;
                case "bad":
                case "b":
                    result.DumpQuality = DumpQuality.Bad;
                    return;
                case "overdump":
                case "o":
                    result.DumpQuality = DumpQuality.Overdump;
                    return;
                case "underdump":
                    result.DumpQuality = DumpQuality.Underdump;
                    return;
                case "mod":
                    result.DumpQuality = DumpQuality.Modified;
                    return;
                case "hack":
                    result.Modification = RomModificationType.Hack;
                    return;
                case "homebrew":
                    result.Modification = RomModificationType.Homebrew;
                    return;
                case "unl":
                    result.Modification = RomModificationType.Unlicensed;
                    return;
                case "proto":
                    result.ReleaseType = RomReleaseType.Prototype;
                    return;
            }

            // Translation tags: [T+Eng], [T-Fre]
            if (tag.StartsWith("T+", StringComparison.OrdinalIgnoreCase) ||
                tag.StartsWith("T-", StringComparison.OrdinalIgnoreCase))
            {
                result.Modification = RomModificationType.Translation;
                var langCode = tag.Substring(2);
                if (LanguageMap.TryGetValue(langCode, out var lang))
                {
                    if (!result.Languages.Contains(lang))
                    {
                        result.Languages.Add(lang);
                    }
                }

                return;
            }
        }

        private static List<Language> InferLanguagesFromRegion(RomRegion region)
        {
            return region switch
            {
                RomRegion.USA => new List<Language> { Language.English },
                RomRegion.Japan => new List<Language> { Language.Japanese },
                RomRegion.Korea => new List<Language> { Language.Korean },
                RomRegion.Brazil => new List<Language> { Language.Portuguese },
                RomRegion.World => new List<Language> { Language.English },
                _ => new List<Language>()
            };
        }

        /// <summary>
        /// Build a No-Intro compliant filename from parsed metadata.
        /// </summary>
        public static string BuildFileName(NoIntroParseResult parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.Title))
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.Append(parsed.Title);

            // Region
            if (parsed.Region != RomRegion.Unknown)
            {
                sb.Append(" (");
                sb.Append(parsed.Region.ToString());
                sb.Append(')');
            }

            // Languages (only if explicitly set and not just inferred)
            if (parsed.Languages.Count > 1 ||
                (parsed.Languages.Count == 1 && !IsDefaultLanguageForRegion(parsed.Languages[0], parsed.Region)))
            {
                var langCodes = parsed.Languages
                    .Select(GetLanguageCode)
                    .Where(c => c != null);
                var langStr = string.Join(",", langCodes);

                if (!string.IsNullOrEmpty(langStr))
                {
                    sb.Append(" (");
                    sb.Append(langStr);
                    sb.Append(')');
                }
            }

            // Revision
            if (!string.IsNullOrWhiteSpace(parsed.Revision))
            {
                sb.Append(" (");
                if (parsed.Revision.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(parsed.Revision);
                }
                else
                {
                    sb.Append("Rev ");
                    sb.Append(parsed.Revision);
                }

                sb.Append(')');
            }

            // Release type
            switch (parsed.ReleaseType)
            {
                case RomReleaseType.Beta:
                    sb.Append(" (Beta)");
                    break;
                case RomReleaseType.Prototype:
                    sb.Append(" (Proto)");
                    break;
                case RomReleaseType.Demo:
                    sb.Append(" (Demo)");
                    break;
                case RomReleaseType.Sample:
                    sb.Append(" (Sample)");
                    break;
                case RomReleaseType.Promo:
                    sb.Append(" (Promo)");
                    break;
                case RomReleaseType.Competition:
                    sb.Append(" (Competition)");
                    break;
                case RomReleaseType.Update:
                    sb.Append(" (Update");
                    if (!string.IsNullOrWhiteSpace(parsed.PatchVersion))
                    {
                        sb.Append(" v");
                        sb.Append(parsed.PatchVersion);
                    }

                    sb.Append(')');
                    break;
                case RomReleaseType.Dlc:
                    sb.Append(" (DLC");
                    if (!string.IsNullOrWhiteSpace(parsed.DlcName))
                    {
                        sb.Append(' ');
                        sb.Append(parsed.DlcName);
                    }

                    sb.Append(')');
                    break;
            }

            // Modification
            if (parsed.Modification == RomModificationType.Hack)
            {
                sb.Append(" (Hack)");
                if (!string.IsNullOrWhiteSpace(parsed.ModificationName))
                {
                    sb.Append(" (");
                    sb.Append(parsed.ModificationName);
                    sb.Append(')');
                }
            }
            else if (parsed.Modification == RomModificationType.Translation)
            {
                sb.Append(" [T+");
                sb.Append(parsed.Languages.Count > 0 ? GetLanguageCode(parsed.Languages[0]) ?? "Eng" : "Eng");
                sb.Append(']');
            }
            else if (parsed.Modification == RomModificationType.Homebrew)
            {
                sb.Append(" [Homebrew]");
            }
            else if (parsed.Modification == RomModificationType.Unlicensed)
            {
                sb.Append(" [Unl]");
            }

            // Dump quality
            switch (parsed.DumpQuality)
            {
                case DumpQuality.Verified:
                    sb.Append(" [!]");
                    break;
                case DumpQuality.Bad:
                    sb.Append(" [bad]");
                    break;
                case DumpQuality.Overdump:
                    sb.Append(" [overdump]");
                    break;
                case DumpQuality.Underdump:
                    sb.Append(" [underdump]");
                    break;
                case DumpQuality.Modified:
                    sb.Append(" [mod]");
                    break;
            }

            // Extension
            if (!string.IsNullOrWhiteSpace(parsed.Extension))
            {
                sb.Append('.');
                sb.Append(parsed.Extension);
            }

            return sb.ToString();
        }

        private static bool IsDefaultLanguageForRegion(Language lang, RomRegion region)
        {
            return region switch
            {
                RomRegion.USA => lang == Language.English,
                RomRegion.Japan => lang == Language.Japanese,
                RomRegion.Korea => lang == Language.Korean,
                RomRegion.Brazil => lang == Language.Portuguese,
                RomRegion.World => lang == Language.English,
                _ => false
            };
        }

        private static string GetLanguageCode(Language lang)
        {
            if (lang == Language.English)
            {
                return "En";
            }

            if (lang == Language.French)
            {
                return "Fr";
            }

            if (lang == Language.German)
            {
                return "De";
            }

            if (lang == Language.Spanish)
            {
                return "Es";
            }

            if (lang == Language.Italian)
            {
                return "It";
            }

            if (lang == Language.Japanese)
            {
                return "Ja";
            }

            if (lang == Language.Portuguese)
            {
                return "Pt";
            }

            if (lang == Language.Russian)
            {
                return "Ru";
            }

            if (lang == Language.Chinese)
            {
                return "Zh";
            }

            if (lang == Language.Korean)
            {
                return "Ko";
            }

            if (lang == Language.Dutch)
            {
                return "Nl";
            }

            if (lang == Language.Swedish)
            {
                return "Sv";
            }

            if (lang == Language.Norwegian)
            {
                return "No";
            }

            if (lang == Language.Danish)
            {
                return "Da";
            }

            if (lang == Language.Finnish)
            {
                return "Fi";
            }

            if (lang == Language.Polish)
            {
                return "Pl";
            }

            if (lang == Language.Turkish)
            {
                return "Tr";
            }

            return null;
        }
    }
}
