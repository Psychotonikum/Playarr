using System;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class FullSeasonSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public FullSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedRomInfo.FullSeason)
            {
                _logger.Debug("Checking if all roms in full platform release have aired. {0}", subject.Release.Title);

                if (subject.Roms.Any(e => !e.AirDateUtc.HasValue || e.AirDateUtc.Value.After(DateTime.UtcNow.AddHours(24))))
                {
                    _logger.Debug("Full platform release {0} rejected. All roms haven't aired yet.", subject.Release.Title);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.FullSeasonNotAired, "Full platform release rejected. All roms haven't aired yet.");
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
