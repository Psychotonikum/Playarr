using System.Linq;
using NLog;
using Playarr.Core.CustomFormats;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class UpgradeAllowedSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeAllowedSpecification(UpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatService,
                                           Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            var qualityProfile = subject.Game.QualityProfile.Value;

            foreach (var file in subject.Roms.Where(c => c.EpisodeFileId != 0).Select(c => c.RomFile.Value))
            {
                if (file == null)
                {
                    _logger.Debug("File is no longer available, skipping this file.");
                    continue;
                }

                var fileCustomFormats = _formatService.ParseCustomFormat(file, subject.Game);

                _logger.Debug("Comparing file quality with report. Existing file is {0}", file.Quality);

                if (!_upgradableSpecification.IsUpgradeAllowed(qualityProfile,
                                                               file.Quality,
                                                               fileCustomFormats,
                                                               subject.ParsedRomInfo.Quality,
                                                               subject.CustomFormats))
                {
                    _logger.Debug("Upgrading is not allowed by the quality profile");

                    return DownloadSpecDecision.Reject(DownloadRejectionReason.QualityUpgradesDisabled, "Existing file and the Quality profile does not allow upgrades");
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
