using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class SceneMappingSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SceneMappingSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Temporary; // Temporary till there's a mapping

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, ReleaseDecisionInformation information)
        {
            if (remoteRom.SceneMapping == null)
            {
                _logger.Debug("No applicable scene mapping, skipping.");
                return DownloadSpecDecision.Accept();
            }

            if (remoteRom.SceneMapping.SceneOrigin.IsNullOrWhiteSpace())
            {
                _logger.Debug("No explicit scene origin in scene mapping.");
                return DownloadSpecDecision.Accept();
            }

            var split = remoteRom.SceneMapping.SceneOrigin.Split(':');

            var isInteractive = information.SearchCriteria is { InteractiveSearch: true };

            if (remoteRom.SceneMapping.Comment.IsNotNullOrWhiteSpace())
            {
                _logger.Debug("SceneMapping has origin {0} with comment '{1}'.", remoteRom.SceneMapping.SceneOrigin, remoteRom.SceneMapping.Comment);
            }
            else
            {
                _logger.Debug("SceneMapping has origin {0}.", remoteRom.SceneMapping.SceneOrigin);
            }

            if (split[0] == "mixed")
            {
                _logger.Debug("SceneMapping origin is explicitly mixed, this means these were released with multiple unidentifiable numbering schemes.");

                if (remoteRom.SceneMapping.Comment.IsNotNullOrWhiteSpace())
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.AmbiguousNumbering, "{0} has ambiguous numbering");
                }
                else
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.AmbiguousNumbering, "Ambiguous numbering");
                }
            }

            if (split[0] == "unknown")
            {
                var type = split.Length >= 2 ? split[1] : "scene";

                _logger.Debug("SceneMapping origin is explicitly unknown, unsure what numbering scheme it uses but '{0}' will be assumed. Provide full release title to Playarr/TheXEM team.", type);
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
