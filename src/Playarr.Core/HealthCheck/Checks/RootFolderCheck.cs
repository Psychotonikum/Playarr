using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.RootFolders;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(SeriesDeletedEvent))]
    [CheckOn(typeof(SeriesMovedEvent))]
    [CheckOn(typeof(EpisodeImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(EpisodeImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RootFolderCheck : HealthCheckBase
    {
        private readonly IGameService _seriesService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;

        public RootFolderCheck(IGameService seriesService, IDiskProvider diskProvider, IRootFolderService rootFolderService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _seriesService = seriesService;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
        }

        public override HealthCheck Check()
        {
            var rootFolders = _seriesService.GetAllSeriesPaths()
                .Select(s => _rootFolderService.GetBestRootFolderPath(s.Value))
                .Distinct()
                .ToList();

            var missingRootFolders = rootFolders.Where(s => !s.IsPathValid(PathValidationType.CurrentOs) || !_diskProvider.FolderExists(s))
                .ToList();

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        HealthCheckReason.RootFolderMissing,
                        _localizationService.GetLocalizedString(
                            "RootFolderMissingHealthCheckMessage",
                            new Dictionary<string, object>
                            {
                                { "rootFolderPath", missingRootFolders.First() }
                            }),
                        "#missing-root-folder");
                }

                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.RootFolderMultipleMissing,
                    _localizationService.GetLocalizedString(
                        "RootFolderMultipleMissingHealthCheckMessage",
                        new Dictionary<string, object>
                        {
                            { "rootFolderPaths", string.Join(" | ", missingRootFolders) }
                        }),
                    "#missing-root-folder");
            }

            var emptyRootFolders = rootFolders
                .Where(r => _diskProvider.FolderEmpty(r))
                .ToList();

            if (emptyRootFolders.Any())
            {
                if (emptyRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Warning,
                        HealthCheckReason.RootFolderEmpty,
                        _localizationService.GetLocalizedString(
                            "RootFolderEmptyHealthCheckMessage",
                            new Dictionary<string, object>
                            {
                                { "rootFolderPath", emptyRootFolders.First() }
                            }),
                        "#empty-root-folder");
                }

                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.RootFolderEmpty,
                    _localizationService.GetLocalizedString(
                        "RootFolderMultipleEmptyHealthCheckMessage",
                        new Dictionary<string, object>
                        {
                            { "rootFolderPaths", string.Join(" | ", emptyRootFolders) }
                        }),
                    "#empty-root-folder");
            }

            return new HealthCheck(GetType());
        }
    }
}
