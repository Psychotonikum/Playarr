using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Localization;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(SeriesUpdatedEvent))]
    [CheckOn(typeof(SeriesDeletedEvent))]
    [CheckOn(typeof(SeriesRefreshCompleteEvent))]
    public class RemovedSeriesCheck : HealthCheckBase, ICheckOnCondition<SeriesUpdatedEvent>, ICheckOnCondition<SeriesDeletedEvent>
    {
        private readonly IGameService _seriesService;

        public RemovedSeriesCheck(IGameService seriesService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _seriesService = seriesService;
        }

        public override HealthCheck Check()
        {
            var deletedSeries = _seriesService.GetAllSeries().Where(v => v.Status == GameStatusType.Deleted).ToList();

            if (deletedSeries.Empty())
            {
                return new HealthCheck(GetType());
            }

            var seriesText = deletedSeries.Select(s => $"{s.Title} (igdbid {s.IgdbId})").Join(", ");

            if (deletedSeries.Count == 1)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.RemovedSeriesSingle,
                    _localizationService.GetLocalizedString("RemovedSeriesSingleRemovedHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "game", seriesText }
                    }),
                    "#game-removed-from-theigdb");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                HealthCheckReason.RemovedSeriesMultiple,
                _localizationService.GetLocalizedString("RemovedSeriesMultipleRemovedHealthCheckMessage", new Dictionary<string, object>
                {
                    { "game", seriesText }
                }),
                "#game-removed-from-theigdb");
        }

        public bool ShouldCheckOnEvent(SeriesDeletedEvent deletedEvent)
        {
            return deletedEvent.Game.Any(s => s.Status == GameStatusType.Deleted);
        }

        public bool ShouldCheckOnEvent(SeriesUpdatedEvent updatedEvent)
        {
            return updatedEvent.Game.Status == GameStatusType.Deleted;
        }
    }
}
