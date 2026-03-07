using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesGrabSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesGrabSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var releaseInfo = localRom.Release;

            if (releaseInfo == null || releaseInfo.RomIds.Empty())
            {
                return ImportSpecDecision.Accept();
            }

            var unexpected = localRom.Roms.Where(e => releaseInfo.RomIds.All(o => o != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected rom(s) in file: {0}", FormatEpisode(unexpected));

                if (unexpected.Count == 1)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeNotFoundInRelease, "Rom {0} was not found in the grabbed release: {1}", FormatEpisode(unexpected), releaseInfo.Title);
                }

                return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeNotFoundInRelease, "Roms {0} were not found in the grabbed release: {1}", FormatEpisode(unexpected), releaseInfo.Title);
            }

            return ImportSpecDecision.Accept();
        }

        private string FormatEpisode(List<Rom> roms)
        {
            return string.Join(", ", roms.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber:00}"));
        }
    }
}
