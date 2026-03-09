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
        private readonly IMetadataSourceProviderRepository _providerRepository;
        private readonly Logger _logger;

        public MetadataSourceProviderFactory(IMetadataSourceProviderRepository providerRepository,
                                             IEnumerable<IMetadataSourceProvider> providers,
                                             IServiceProvider container,
                                             IEventAggregator eventAggregator,
                                             Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        protected override void InitializeProviders()
        {
            var definitions = new List<MetadataSourceDefinition>();

            foreach (var provider in _providers)
            {
                // Skip IGDB — it requires manual configuration (Twitch credentials)
                if (provider is IgdbProvider)
                {
                    continue;
                }

                definitions.Add(new MetadataSourceDefinition
                {
                    Name = provider.Name,
                    EnableSearch = provider.SupportsSearch,
                    EnableCalendar = provider.SupportsCalendar,
                    DownloadMetadata = false,
                    Implementation = provider.GetType().Name,
                    Settings = (IProviderConfig)Activator.CreateInstance(provider.ConfigContract)
                });
            }

            var currentProviders = All();
            var newProviders = definitions.Where(def => currentProviders.All(c => c.Implementation != def.Implementation)).ToList();

            if (newProviders.Any())
            {
                _providerRepository.InsertMany(newProviders.Cast<MetadataSourceDefinition>().ToList());
            }
        }

        protected override List<MetadataSourceDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override void SetProviderCharacteristics(IMetadataSourceProvider provider, MetadataSourceDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.SupportsSearch = provider.SupportsSearch;
            definition.SupportsCalendar = provider.SupportsCalendar;
            definition.SupportsMetadataDownload = provider.SupportsMetadataDownload;
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
