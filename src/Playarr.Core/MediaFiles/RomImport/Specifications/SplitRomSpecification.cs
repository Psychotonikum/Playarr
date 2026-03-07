using NLog;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class SplitEpisodeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.FileRomInfo == null)
            {
                return ImportSpecDecision.Accept();
            }

            if (localRom.FileRomInfo.IsSplitEpisode)
            {
                _logger.Debug("Single rom split into multiple files");
                return ImportSpecDecision.Reject(ImportRejectionReason.SplitEpisode, "Single rom split into multiple files");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
