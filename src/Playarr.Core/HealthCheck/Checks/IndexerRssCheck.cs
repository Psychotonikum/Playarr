using Playarr.Common.Extensions;
using Playarr.Core.Indexers;
using Playarr.Core.Localization;
using Playarr.Core.ThingiProvider.Events;

namespace Playarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderAddedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderUpdatedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderDeletedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IIndexer>))]
    public class IndexerRssCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerRssCheck(IIndexerFactory indexerFactory, ILocalizationService localizationService)
            : base(localizationService)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var enabled = _indexerFactory.RssEnabled(false);

            if (enabled.Empty())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.IndexerRssNoIndexersEnabled,
                    _localizationService.GetLocalizedString("IndexerRssNoIndexersEnabledHealthCheckMessage"),
                    "#no-indexers-available-with-rss-sync-enabled-playarr-will-not-grab-new-releases-automatically");
            }

            var active = _indexerFactory.RssEnabled(true);

            if (active.Empty())
            {
                 return new HealthCheck(GetType(),
                     HealthCheckResult.Warning,
                     HealthCheckReason.IndexerRssNoIndexersAvailable,
                     _localizationService.GetLocalizedString("IndexerRssNoIndexersAvailableHealthCheckMessage"),
                     "#indexers-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType());
        }
    }
}
