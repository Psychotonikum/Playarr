using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class QualityAllowedByProfileSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public QualityAllowedByProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            _logger.Debug("Checking if report meets quality requirements. {0}", subject.ParsedRomInfo.Quality);

            var profile = subject.Game.QualityProfile.Value;
            var qualityIndex = profile.GetIndex(subject.ParsedRomInfo.Quality.Quality);
            var qualityOrGroup = profile.Items[qualityIndex.Index];

            if (!qualityOrGroup.Allowed)
            {
                _logger.Debug("Quality {0} rejected by Game' quality profile", subject.ParsedRomInfo.Quality);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.QualityNotWanted, "{0} is not wanted in profile", subject.ParsedRomInfo.Quality.Quality);
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
