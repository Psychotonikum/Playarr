using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromMediaInfo : IAugmentLanguage
    {
        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentLanguageResult AugmentLanguage(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.MediaInfo == null)
            {
                return null;
            }

            var audioLanguages = localRom.MediaInfo.AudioStreams?.Select(l => l.Language).Distinct().ToList() ?? [];

            var languages = new List<Languages.Language>();

            foreach (var audioLanguage in audioLanguages)
            {
                var language = IsoLanguages.Find(audioLanguage)?.Language;
                languages.AddIfNotNull(language);
            }

            if (languages.Count == 0)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.MediaInfo);
        }
    }
}
