using System.Collections.Generic;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }

        IEnumerable<int> MultiLanguages { get; set; }

        IEnumerable<int> FailDownloads { get; set; }
    }
}
