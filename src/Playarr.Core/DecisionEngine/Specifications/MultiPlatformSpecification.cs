using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class MultiSeasonSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MultiSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedRomInfo.IsMultiSeason)
            {
                _logger.Debug("Multi-platform release {0} rejected. Not supported", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MultiSeason, "Multi-platform releases are not supported");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
