using NLog;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class SameEpisodesImportSpecification : IImportDecisionEngineSpecification
    {
        private readonly SameEpisodesSpecification _sameEpisodesSpecification;
        private readonly Logger _logger;

        public SameEpisodesImportSpecification(SameEpisodesSpecification sameEpisodesSpecification, Logger logger)
        {
            _sameEpisodesSpecification = sameEpisodesSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (_sameEpisodesSpecification.IsSatisfiedBy(localRom.Roms))
            {
                return ImportSpecDecision.Accept();
            }

            _logger.Debug("Rom file on disk contains more roms than this file contains");
            return ImportSpecDecision.Reject(ImportRejectionReason.ExistingFileHasMoreEpisodes, "Rom file on disk contains more roms than this file contains");
        }
    }
}
