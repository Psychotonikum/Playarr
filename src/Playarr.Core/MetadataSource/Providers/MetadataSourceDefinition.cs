using Playarr.Core.ThingiProvider;

namespace Playarr.Core.MetadataSource.Providers
{
    public class MetadataSourceDefinition : ProviderDefinition
    {
        public bool EnableSearch { get; set; }
        public bool EnableCalendar { get; set; }
        public bool DownloadMetadata { get; set; }

        public override bool Enable => EnableSearch || EnableCalendar;
    }
}
