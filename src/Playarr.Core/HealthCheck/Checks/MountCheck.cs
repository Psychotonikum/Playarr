using System;
using System.Linq;
using Playarr.Common.Disk;
using Playarr.Core.Localization;
using Playarr.Core.Games;

namespace Playarr.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IGameService _gameService;

        public MountCheck(IDiskProvider diskProvider, IGameService seriesService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _gameService = seriesService;
        }

        public override HealthCheck Check()
        {
            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on game path so internals can handle mount resolution.
            var mounts = _gameService.GetAllGamePaths()
                .Select(p => new Tuple<IMount, string>(_diskProvider.GetMount(p.Value), p.Value))
                .Where(m => m.Item1 is { MountOptions.IsReadOnly: true })
                .DistinctBy(m => m.Item1.RootDirectory)
                .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.MountSeries,
                    $"{_localizationService.GetLocalizedString("MountSeriesHealthCheckMessage")}{string.Join(", ", mounts.Select(m => $"{m.Item1.Name} ({m.Item2})"))}",
                    "#game-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
