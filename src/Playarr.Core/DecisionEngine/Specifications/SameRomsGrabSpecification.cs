using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class SameEpisodesGrabSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly SameEpisodesSpecification _sameEpisodesSpecification;
        private readonly Logger _logger;

        public SameEpisodesGrabSpecification(SameEpisodesSpecification sameEpisodesSpecification, Logger logger)
        {
            _sameEpisodesSpecification = sameEpisodesSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (_sameEpisodesSpecification.IsSatisfiedBy(subject.Roms))
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Rom file on disk contains more roms than this release contains");
            return DownloadSpecDecision.Reject(DownloadRejectionReason.ExistingFileHasMoreEpisodes, "Rom file on disk contains more roms than this release contains");
        }
    }
}
