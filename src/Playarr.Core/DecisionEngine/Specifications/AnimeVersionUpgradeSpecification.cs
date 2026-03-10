using NLog;
using Playarr.Core.Configuration;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class AnimeVersionUpgradeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AnimeVersionUpgradeSpecification(UpgradableSpecification upgradableSpecification, IConfigService configService, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            // Anime version upgrade logic removed - not applicable to game ROMs
            return DownloadSpecDecision.Accept();
        }
    }
}
