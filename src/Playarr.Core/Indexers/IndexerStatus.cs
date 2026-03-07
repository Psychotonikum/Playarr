using Playarr.Core.Parser.Model;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.Indexers
{
    public class IndexerStatus : ProviderStatusBase
    {
        public ReleaseInfo LastRssSyncReleaseInfo { get; set; }
    }
}
