using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Configuration;
using Playarr.Core.Profiles.Delay;

namespace Playarr.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        private readonly IConfigService _configService;
        private readonly IDelayProfileService _delayProfileService;

        public DownloadDecisionPriorizationService(IConfigService configService, IDelayProfileService delayProfileService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteRom.Game != null)
                            .GroupBy(c => c.RemoteRom.Game.Id, (gameId, downloadDecisions) =>
                                {
                                    return downloadDecisions.OrderByDescending(decision => decision, new DownloadDecisionComparer(_configService, _delayProfileService));
                                })
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteRom.Game == null))
                            .ToList();
        }
    }
}
