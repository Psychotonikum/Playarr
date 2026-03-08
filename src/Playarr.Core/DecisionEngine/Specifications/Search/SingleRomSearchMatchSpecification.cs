using System.Linq;
using NLog;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.Search
{
    public class SingleEpisodeSearchMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public SingleEpisodeSearchMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteRom, ReleaseDecisionInformation information)
        {
            var searchCriteria = information.SearchCriteria;

            if (searchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (searchCriteria is SingleEpisodeSearchCriteria singleEpisodeSpec)
            {
                return IsSatisfiedBy(remoteRom, singleEpisodeSpec);
            }

            if (searchCriteria is AnimeEpisodeSearchCriteria animeEpisodeSpec)
            {
                return IsSatisfiedBy(remoteRom, animeEpisodeSpec);
            }

            return DownloadSpecDecision.Accept();
        }

        private DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteRom, SingleEpisodeSearchCriteria singleEpisodeSpec)
        {
            if (singleEpisodeSpec.PlatformNumber != remoteRom.ParsedRomInfo.PlatformNumber)
            {
                _logger.Debug("Platform number does not match searched platform number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeason, "Wrong platform");
            }

            if (!remoteRom.ParsedRomInfo.RomNumbers.Any())
            {
                _logger.Debug("Full platform result during single rom search, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.FullSeason, "Full platform pack");
            }

            if (!remoteRom.ParsedRomInfo.RomNumbers.Contains(singleEpisodeSpec.EpisodeNumber))
            {
                _logger.Debug("Rom number does not match searched rom number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongEpisode, "Wrong rom");
            }

            return DownloadSpecDecision.Accept();
        }

        private DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteRom, AnimeEpisodeSearchCriteria animeEpisodeSpec)
        {
            if (remoteRom.ParsedRomInfo.FullSeason && !animeEpisodeSpec.IsSeasonSearch)
            {
                _logger.Debug("Full platform result during single rom search, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.FullSeason, "Full platform pack");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
