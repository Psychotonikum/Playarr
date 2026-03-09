using Playarr.Core.ThingiProvider;

namespace Playarr.Core.MetadataSource.Providers
{
    public interface IMetadataSourceProvider : IProvider
    {
        bool SupportsSearch { get; }
        bool SupportsCalendar { get; }
        bool SupportsMetadataDownload { get; }
    }
}
