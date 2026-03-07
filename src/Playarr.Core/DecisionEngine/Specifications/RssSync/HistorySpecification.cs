using System;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFormats;
using Playarr.Core.History;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public HistorySpecification(IHistoryService historyService,
                                    UpgradableSpecification upgradableSpecification,
                                    ICustomFormatCalculationService formatService,
                                    IConfigService configService,
                                    Logger logger)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return DownloadSpecDecision.Accept();
            }

            var cdhEnabled = _configService.EnableCompletedDownloadHandling;
            var qualityProfile = subject.Game.QualityProfile.Value;

            _logger.Debug("Performing history status check on report");

            foreach (var rom in subject.Roms)
            {
                _logger.Debug("Checking current status of rom [{0}] in history", rom.Id);
                var mostRecent = _historyService.MostRecentForEpisode(rom.Id);

                if (mostRecent != null && mostRecent.EventType == EpisodeHistoryEventType.Grabbed)
                {
                    var recent = mostRecent.Date.After(DateTime.UtcNow.AddHours(-12));

                    if (!recent && cdhEnabled)
                    {
                        continue;
                    }

                    var customFormats = _formatService.ParseCustomFormat(mostRecent, subject.Game);

                    // The game will be the same as the one in history since it's the same rom.
                    // Instead of fetching the game from the DB reuse the known game.
                    var cutoffUnmet = _upgradableSpecification.CutoffNotMet(
                        subject.Game.QualityProfile,
                        mostRecent.Quality,
                        customFormats,
                        subject.ParsedRomInfo.Quality);

                    var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(
                        subject.Game.QualityProfile,
                        mostRecent.Quality,
                        customFormats,
                        subject.ParsedRomInfo.Quality,
                        subject.CustomFormats);

                    if (!cutoffUnmet)
                    {
                        if (recent)
                        {
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryRecentCutoffMet, "Recent grab event in history already meets cutoff: {0}", mostRecent.Quality);
                        }

                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCdhDisabledCutoffMet, "CDH is disabled and grab event in history already meets cutoff: {0}", mostRecent.Quality);
                    }

                    var rejectionSubject = recent ? "Recent" : "CDH is disabled and";

                    switch (upgradeableRejectReason)
                    {
                        case UpgradeableRejectReason.None:
                            continue;

                        case UpgradeableRejectReason.BetterQuality:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherPreference, "{0} grab event in history is of equal or higher preference: {1}", rejectionSubject, mostRecent.Quality);

                        case UpgradeableRejectReason.BetterRevision:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherRevision, "{0} grab event in history is of equal or higher revision: {1}", rejectionSubject, mostRecent.Quality.Revision);

                        case UpgradeableRejectReason.QualityCutoff:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCutoffMet, "{0} grab event in history meets quality cutoff: {1}", rejectionSubject, qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                        case UpgradeableRejectReason.CustomFormatCutoff:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatCutoffMet, "{0} grab event in history meets Custom Format cutoff: {1}", rejectionSubject, qualityProfile.CutoffFormatScore);

                        case UpgradeableRejectReason.CustomFormatScore:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScore, "{0} grab event in history has an equal or higher Custom Format score: {1}", rejectionSubject, qualityProfile.CalculateCustomFormatScore(customFormats));

                        case UpgradeableRejectReason.MinCustomFormatScore:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScoreIncrement, "{0} grab event in history has Custom Format score within Custom Format score increment: {1}", rejectionSubject, qualityProfile.MinUpgradeFormatScore);

                        case UpgradeableRejectReason.UpgradesNotAllowed:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryUpgradesNotAllowed, "{0} grab event in history and Quality Profile '{1}' does not allow upgrades", rejectionSubject, qualityProfile.Name);
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
