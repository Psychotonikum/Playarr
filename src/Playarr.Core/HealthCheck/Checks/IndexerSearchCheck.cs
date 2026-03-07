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
    public class IndexerSearchCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerSearchCheck(IIndexerFactory indexerFactory, ILocalizationService localizationService)
            : base(localizationService)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var automaticSearchEnabled = _indexerFactory.AutomaticSearchEnabled(false);

            if (automaticSearchEnabled.Empty())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.IndexerSearchNoAutomatic,
                    _localizationService.GetLocalizedString("IndexerSearchNoAutomaticHealthCheckMessage"),
                    "#no-indexers-available-with-automatic-search-enabled-playarr-will-not-provide-any-automatic-search-results");
            }

            var interactiveSearchEnabled = _indexerFactory.InteractiveSearchEnabled(false);

            if (interactiveSearchEnabled.Empty())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.IndexerSearchNoInteractive,
                    _localizationService.GetLocalizedString("IndexerSearchNoInteractiveHealthCheckMessage"),
                    "#no-indexers-available-with-interactive-search-enabled");
            }

            var active = _indexerFactory.AutomaticSearchEnabled(true);

            if (active.Empty())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.IndexerSearchNoAvailableIndexers,
                    _localizationService.GetLocalizedString("IndexerSearchNoAvailableIndexersHealthCheckMessage"),
                    "#indexers-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType());
        }
    }
}
