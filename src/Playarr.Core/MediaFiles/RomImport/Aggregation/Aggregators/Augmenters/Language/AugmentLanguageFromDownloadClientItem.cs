using System.Linq;
using Playarr.Core.Download;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromDownloadClientItem : IAugmentLanguage
    {
        public int Order => 3;
        public string Name => "DownloadClientItem";

        public AugmentLanguageResult AugmentLanguage(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var languages = localRom.DownloadClientRomInfo?.Languages;

            if (languages == null)
            {
                return null;
            }

            foreach (var rom in localRom.Roms)
            {
                var romTitleLanguage = LanguageParser.ParseLanguages(rom.Title);

                languages = languages.Except(romTitleLanguage).ToList();
            }

            return new AugmentLanguageResult(languages, Confidence.DownloadClientItem);
        }
    }
}
