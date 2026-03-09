using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.MetadataSource.Providers
{
    public abstract class MetadataSourceProviderBase<TSettings> : IMetadataSourceProvider
        where TSettings : IProviderConfig, new()
    {
        protected readonly Logger _logger;

        protected MetadataSourceProviderBase(Logger logger)
        {
            _logger = logger;
        }

        public abstract string Name { get; }

        public abstract bool SupportsSearch { get; }
        public abstract bool SupportsCalendar { get; }
        public abstract bool SupportsMetadataDownload { get; }

        public Type ConfigContract => typeof(TSettings);

        public ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new MetadataSourceDefinition
                {
                    Name = Name,
                    EnableSearch = true,
                    EnableCalendar = SupportsCalendar,
                    DownloadMetadata = false,
                    Implementation = GetType().Name,
                    Settings = new TSettings()
                };
            }
        }

        public ProviderDefinition Definition { get; set; }

        public virtual ValidationResult Test()
        {
            return new ValidationResult();
        }

        public virtual object RequestAction(string stage, IDictionary<string, string> query)
        {
            return new { };
        }
    }
}
