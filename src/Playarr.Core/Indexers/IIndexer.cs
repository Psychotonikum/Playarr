using System.Collections.Generic;
using System.Threading.Tasks;
using Playarr.Common.Http;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser.Model;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        DownloadProtocol Protocol { get; }

        Task<IList<ReleaseInfo>> FetchRecent();
        Task<IList<ReleaseInfo>> Fetch(SeasonSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(SingleEpisodeSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(SpecialEpisodeSearchCriteria searchCriteria);
        HttpRequest GetDownloadRequest(string link);
    }
}
