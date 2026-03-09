using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.MetadataSource.Providers
{
    public interface IMetadataSourceProviderFactory : IProviderFactory<IMetadataSourceProvider, MetadataSourceDefinition>
    {
        List<IMetadataSourceProvider> SearchEnabled();
        List<IMetadataSourceProvider> CalendarEnabled();
    }

    public class MetadataSourceProviderFactory : ProviderFactory<IMetadataSourceProvider, MetadataSourceDefinition>, IMetadataSourceProviderFactory
    {
        private readonly Logger _logger;

        public MetadataSourceProviderFactory(IMetadataSourceProviderRepository providerRepository,
                                             IEnumerable<IMetadataSourceProvider> providers,
                                             IServiceProvider container,
                                             IEventAggregator eventAggregator,
                                             Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _logger = logger;
        }

        protected override List<MetadataSourceDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override void SetProviderCharacteristics(IMetadataSourceProvider provider, MetadataSourceDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);
        }

        public List<IMetadataSourceProvider> SearchEnabled()
        {
            return GetAvailableProviders()
                .Where(p => ((MetadataSourceDefinition)p.Definition).EnableSearch)
                .ToList();
        }

        public List<IMetadataSourceProvider> CalendarEnabled()
        {
            return GetAvailableProviders()
                .Where(p => ((MetadataSourceDefinition)p.Definition).EnableCalendar)
                .ToList();
        }
    }
}
