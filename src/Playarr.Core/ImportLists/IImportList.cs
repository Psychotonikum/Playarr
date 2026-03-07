using System;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.ImportLists
{
    public interface IImportList : IProvider
    {
        ImportListType ListType { get; }
        TimeSpan MinRefreshInterval { get; }
        ImportListFetchResult Fetch();
    }
}
