using System;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class SeasonPackOnlySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SeasonPackOnlySpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            var searchCriteria = information.SearchCriteria;

            if (searchCriteria == null || searchCriteria.Roms.Count == 1)
            {
                return DownloadSpecDecision.Accept();
            }

            if (subject.Release.SeasonSearchMaximumSingleEpisodeAge > 0)
            {
                if (subject.Game.SeriesType == GameTypes.Standard && !subject.ParsedRomInfo.FullSeason && subject.Roms.Count >= 1)
                {
                    // test against roms of the same platform in the current search, and make sure they have an air date
                    var subset = searchCriteria.Roms.Where(e => e.AirDateUtc.HasValue && e.SeasonNumber == subject.Roms.First().SeasonNumber).ToList();

                    if (subset.Count > 0 && subset.Max(e => e.AirDateUtc).Value.Before(DateTime.UtcNow - TimeSpan.FromDays(subject.Release.SeasonSearchMaximumSingleEpisodeAge)))
                    {
                        _logger.Debug("Release {0}: last rom in this platform aired more than {1} days ago, platform pack required.", subject.Release.Title, subject.Release.SeasonSearchMaximumSingleEpisodeAge);
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.NotSeasonPack, "Last rom in this platform aired more than {0} days ago, platform pack required.", subject.Release.SeasonSearchMaximumSingleEpisodeAge);
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
