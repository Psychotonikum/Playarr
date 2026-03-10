using NLog;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.Search
{
    public class SeasonMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public SeasonMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var singleEpisodeSpec = information.SearchCriteria as SeasonSearchCriteria;

            if (singleEpisodeSpec == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (singleEpisodeSpec.PlatformNumber != remoteRom.ParsedRomInfo.PlatformNumber)
            {
                _logger.Debug("Platform number does not match searched platform number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeason, "Wrong platform");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
