using Playarr.Core.ThingiProvider;

namespace Playarr.Core.ImportLists
{
    public interface IImportListSettings : IProviderConfig
    {
        string BaseUrl { get; set; }
    }
}
