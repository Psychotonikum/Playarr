using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.Search
{
    public class SeriesSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SeriesSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteRom remoteRom, ReleaseDecisionInformation information)
        {
            var searchCriteria = information.SearchCriteria;

            if (searchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Checking if game matches searched game");

            if (remoteRom.Game.Id != searchCriteria.Game.Id)
            {
                _logger.Debug("Game {0} does not match {1}", remoteRom.Game, searchCriteria.Game);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeries, "Wrong game");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
