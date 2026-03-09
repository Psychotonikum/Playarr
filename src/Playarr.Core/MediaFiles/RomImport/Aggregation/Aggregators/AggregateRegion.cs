using System.Text.RegularExpressions;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateRegion : IAggregateLocalEpisode
    {
        // Match region codes commonly found in ROM filenames:
        // No-Intro style: (USA), (Europe), (Japan), (World), (USA, Europe)
        // Aerofoil style: Game Title USA.ext
        // Scene style: NTSC, PAL, NTSC-U, NTSC-J
        private static readonly Regex RegionRegex = new Regex(
            @"\b(USA|Europe|Japan|World|EUR|JPN|JAP|KOR|CHN|TWN|AUS|BRA|FRA|GER|ITA|SPA|NTSC|PAL|NTSC-[JUPE])\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public int Order => 3;

        public LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(localRom.Path);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var match = RegionRegex.Match(fileName);

                if (match.Success)
                {
                    localRom.Region = NormalizeRegion(match.Value);
                }
            }

            return localRom;
        }

        private static string NormalizeRegion(string region)
        {
            var upper = region.ToUpperInvariant();

            return upper switch
            {
                "JAP" => "Japan",
                "JPN" => "Japan",
                "NTSC-J" => "Japan",
                "EUR" => "Europe",
                "PAL" => "Europe",
                "NTSC-E" => "Europe",
                "USA" => "USA",
                "NTSC" => "USA",
                "NTSC-U" => "USA",
                "KOR" => "Korea",
                "CHN" => "China",
                "TWN" => "Taiwan",
                "AUS" => "Australia",
                "BRA" => "Brazil",
                "FRA" => "France",
                "GER" => "Germany",
                "ITA" => "Italy",
                "SPA" => "Spain",
                _ => region
            };
        }
    }
}
