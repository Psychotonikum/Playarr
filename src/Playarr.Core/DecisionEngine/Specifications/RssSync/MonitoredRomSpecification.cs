using System.Linq;
using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.RssSync
{
    public class MonitoredEpisodeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria is { MonitoredEpisodesOnly: false })
            {
                _logger.Debug("Skipping monitored check during search");
                return DownloadSpecDecision.Accept();
            }

            if (!subject.Game.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. Rejecting", subject.Game);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.SeriesNotMonitored, "Game is not monitored");
            }

            var monitoredCount = subject.Roms.Count(rom => rom.Monitored);
            if (monitoredCount == subject.Roms.Count)
            {
                return DownloadSpecDecision.Accept();
            }

            if (subject.Roms.Count == 1)
            {
                _logger.Debug("Rom is not monitored. Rejecting", monitoredCount, subject.Roms.Count);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.EpisodeNotMonitored, "Rom is not monitored");
            }

            if (monitoredCount == 0)
            {
                _logger.Debug("No roms in the release are monitored. Rejecting", monitoredCount, subject.Roms.Count);
            }
            else
            {
                _logger.Debug("Only {0}/{1} roms in the release are monitored. Rejecting", monitoredCount, subject.Roms.Count);
            }

            return DownloadSpecDecision.Reject(DownloadRejectionReason.EpisodeNotMonitored, "One or more roms is not monitored");
        }
    }
}
