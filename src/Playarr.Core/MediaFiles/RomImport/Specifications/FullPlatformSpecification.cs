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
                _logger.Debug("Single rom file detected as containing all roms in the platform due to no rom parsed from the file name.");
                return ImportSpecDecision.Reject(ImportRejectionReason.FullSeason, "Single rom file contains all roms in platforms. Review file name or manually import");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
