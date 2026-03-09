using Playarr.Core.ThingiProvider;

namespace Playarr.Core.MetadataSource.Providers
{
    public class MetadataSourceDefinition : ProviderDefinition
    {
        public bool EnableSearch { get; set; }
        public bool EnableCalendar { get; set; }
        public bool DownloadMetadata { get; set; }

        public bool SupportsSearch { get; set; }
        public bool SupportsCalendar { get; set; }
        public bool SupportsMetadataDownload { get; set; }

        public override bool Enable
        {
            get => EnableSearch || EnableCalendar;
            set { /* computed from EnableSearch/EnableCalendar */ }
        }
    }
}
