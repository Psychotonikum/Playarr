using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class SplitEpisodeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedRomInfo.IsSplitEpisode)
            {
                _logger.Debug("Split rom release {0} rejected. Not supported", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.SplitEpisode, "Split rom releases are not supported");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
