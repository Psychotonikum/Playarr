using System;
using Playarr.Core.ThingiProvider.Status;

namespace Playarr.Core.ImportLists
{
    public class ImportListStatus : ProviderStatusBase
    {
        public DateTime? LastInfoSync { get; set; }
        public bool HasRemovedItemSinceLastClean { get; set; }
    }
}
