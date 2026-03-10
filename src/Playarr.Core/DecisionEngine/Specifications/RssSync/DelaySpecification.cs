using System.Linq;
using NLog;
using Playarr.Core.Configuration;
using Playarr.Core.Download.Pending;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Qualities;

namespace Playarr.Core.DecisionEngine.Specifications.RssSync
{
    public class DelaySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DelaySpecification(IPendingReleaseService pendingReleaseService,
                                  IDelayProfileService delayProfileService,
                                  IConfigService configService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _delayProfileService = delayProfileService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria is { UserInvokedSearch: true })
            {
                _logger.Debug("Ignoring delay for user invoked search");
                return DownloadSpecDecision.Accept();
            }

            var qualityProfile = subject.Game.QualityProfile.Value;
            var delayProfile = _delayProfileService.BestForTags(subject.Game.Tags);
            var delay = delayProfile.GetProtocolDelay(subject.Release.DownloadProtocol);
            var isPreferredProtocol = subject.Release.DownloadProtocol == delayProfile.PreferredProtocol;
            var preferPropersAndRepacks = _configService.DownloadPropersAndRepacks == ProperDownloadTypes.PreferAndUpgrade;

            if (delay == 0)
            {
                _logger.Debug("Delay Profile does not require a waiting period before download for {0}.", subject.Release.DownloadProtocol);
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Delay Profile requires a waiting period of {0} minutes for {1}", delay, subject.Release.DownloadProtocol);

            var qualityComparer = new QualityModelComparer(qualityProfile);

            if (isPreferredProtocol && preferPropersAndRepacks)
            {
                foreach (var file in subject.Roms.Where(c => c.EpisodeFileId != 0).Select(c => c.RomFile.Value))
                {
                    var currentQuality = file.Quality;
                    var newQuality = subject.ParsedRomInfo.Quality;
                    var qualityCompare = qualityComparer.Compare(newQuality?.Quality, currentQuality.Quality);

                    if (qualityCompare == 0 && newQuality?.Revision.CompareTo(currentQuality.Revision) > 0)
                    {
                        _logger.Debug("New quality is a better revision for existing quality, skipping delay");
                        return DownloadSpecDecision.Accept();
                    }
                }
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            if (delayProfile.BypassIfHighestQuality)
            {
                var bestQualityInProfile = qualityProfile.LastAllowedQuality();
                var isBestInProfile = qualityComparer.Compare(subject.ParsedRomInfo.Quality.Quality, bestQualityInProfile) >= 0;

                if (isBestInProfile && isPreferredProtocol)
                {
                    _logger.Debug("Quality is highest in profile for preferred protocol, will not delay");
                    return DownloadSpecDecision.Accept();
                }
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            if (delayProfile.BypassIfAboveCustomFormatScore)
            {
                var score = subject.CustomFormatScore;
                var minimum = delayProfile.MinimumCustomFormatScore;

                if (score >= minimum && isPreferredProtocol)
                {
                    _logger.Debug("Custom format score ({0}) meets minimum ({1}) for preferred protocol, will not delay", score, minimum);
                    return DownloadSpecDecision.Accept();
                }
            }

            var romIds = subject.Roms.Select(e => e.Id);

            var oldest = _pendingReleaseService.OldestPendingRelease(subject.Game.Id, romIds.ToArray());

            if (oldest != null && oldest.Release.AgeMinutes > delay)
            {
                _logger.Debug("Oldest pending release {0} has been delayed for {1}, longer than the set delay of {2}. Release will be accepted", oldest.Release.Title, oldest.Release.AgeMinutes, delay);
                return DownloadSpecDecision.Accept();
            }

            if (subject.Release.AgeMinutes < delay)
            {
                _logger.Debug("Waiting for better quality release, There is a {0} minute delay on {1}", delay, subject.Release.DownloadProtocol);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumAgeDelay, "Waiting for better quality release");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
