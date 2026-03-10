using System.Linq;
using NLog;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FullSeasonSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public FullSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.FileRomInfo == null)
            {
                return ImportSpecDecision.Accept();
            }

            if (localRom.FileRomInfo.FullSeason)
            {
                // Accept ROM files in game folders that were matched to roms by platform folder name
                if (localRom.ExistingFile && localRom.Roms.Any())
                {
                    _logger.Debug("ROM file matched to platform via folder name, accepting.");
                    return ImportSpecDecision.Accept();
                }

                _logger.Debug("Single rom file detected as containing all roms in the platform due to no rom parsed from the file name.");
                return ImportSpecDecision.Reject(ImportRejectionReason.FullSeason, "Single rom file contains all roms in platforms. Review file name or manually import");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
