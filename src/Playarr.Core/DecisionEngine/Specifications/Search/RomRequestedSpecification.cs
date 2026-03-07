using System.Linq;
using NLog;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications.Search
{
    public class EpisodeRequestedSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public EpisodeRequestedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteRom, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var criteriaEpisodes = information.SearchCriteria.Roms.Select(v => v.Id).ToList();
            var remoteRoms = remoteRom.Roms.Select(v => v.Id).ToList();

            if (!criteriaEpisodes.Intersect(remoteRoms).Any())
            {
                _logger.Debug("Release rejected since the rom wasn't requested: {0}", remoteRom.ParsedRomInfo);

                if (remoteRoms.Any())
                {
                    var roms = remoteRom.Roms.OrderBy(v => v.SeasonNumber).ThenBy(v => v.EpisodeNumber).ToList();

                    if (roms.Count > 1)
                    {
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongEpisode, $"Rom wasn't requested: {roms.First().SeasonNumber}x{roms.First().EpisodeNumber}-{roms.Last().EpisodeNumber}");
                    }
                    else
                    {
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongEpisode, $"Rom wasn't requested: {roms.First().SeasonNumber}x{roms.First().EpisodeNumber}");
                    }
                }
                else
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongEpisode, "Rom wasn't requested");
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
