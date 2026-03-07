using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IRomService _episodeService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IRomService episodeService, Logger logger)
        {
            _episodeService = episodeService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedRomInfo.Quality.Quality;

            if (subject.ParsedRomInfo.Special)
            {
                _logger.Debug("Special release found, skipping size check.");
                return DownloadSpecDecision.Accept();
            }

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check");
                return DownloadSpecDecision.Accept();
            }

            var seriesRuntime = subject.Game.Runtime;
            var runtime = 0;

            if (seriesRuntime == 0)
            {
                var firstPlatformNumber = subject.Game.Platforms.Where(s => s.SeasonNumber > 0).Min(s => s.SeasonNumber);
                var pilotEpisode = _episodeService.GetEpisodesBySeason(subject.Game.Id, firstPlatformNumber).First();

                if (subject.Roms.First().SeasonNumber == pilotEpisode.SeasonNumber)
                {
                    // If the first rom has an air date use it, otherwise use the release's publish date because like runtime it may not have updated yet.
                    var gracePeriodEnd = (pilotEpisode.AirDateUtc ?? subject.Release.PublishDate).AddHours(24);

                    // If roms don't have an air date that is okay, otherwise make sure it's within 24 hours of the first rom airing.
                    if (subject.Roms.All(e => !e.AirDateUtc.HasValue || e.AirDateUtc.Value.Before(gracePeriodEnd)))
                    {
                        _logger.Debug("Game runtime is 0, but all roms in release aired within 24 hours of first rom in platform, defaulting runtime to 45 minutes");
                        seriesRuntime = 45;
                    }
                }
            }

            // For each rom use the runtime of the rom or fallback to the game runtime
            // (which in turn might have fallen back to a default runtime of 45)
            foreach (var rom in subject.Roms)
            {
                runtime += rom.Runtime > 0 ? rom.Runtime : seriesRuntime;
            }

            // Reject if the run time is 0
            if (runtime == 0)
            {
                _logger.Debug("Runtime of all roms is 0, unable to validate size until it is available, rejecting");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.UnknownRuntime, "Runtime of all roms is 0, unable to validate size until it is available");
            }

            var qualityProfile = subject.Game.QualityProfile.Value;
            var qualityIndex = qualityProfile.GetIndex(quality, true);
            var qualityOrGroup = qualityProfile.Items[qualityIndex.Index];
            var item = qualityOrGroup.Quality == null ? qualityOrGroup.Items[qualityIndex.GroupIndex] : qualityOrGroup;

            if (item.MinSize.HasValue)
            {
                var minSize = item.MinSize.Value.Megabytes();

                // Multiply maxSize by runtime of all roms
                minSize *= runtime;

                // If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = subject.Roms.Count == 1 ? $"{runtime}min" : $"{subject.Roms.Count} roms totalling {runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.BelowMinimumSize, "{0} is smaller than minimum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), runtimeMessage);
                }
            }

            if (!item.MaxSize.HasValue || item.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited, skipping size check");
            }
            else
            {
                var maxSize = item.MaxSize.Value.Megabytes();

                // Multiply maxSize by runtime of all roms
                maxSize *= runtime;

                // If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    var runtimeMessage = subject.Roms.Count == 1 ? $"{runtime}min" : $"{subject.Roms.Count} roms totalling {runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting", subject, subject.Release.Size, maxSize, runtimeMessage);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.AboveMaximumSize, "{0} is larger than maximum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), runtimeMessage);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints", subject);
            return DownloadSpecDecision.Accept();
        }
    }
}
