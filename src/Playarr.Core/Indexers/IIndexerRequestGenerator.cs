using Playarr.Core.IndexerSearch.Definitions;

namespace Playarr.Core.Indexers
{
    public interface IIndexerRequestGenerator
    {
        IndexerPageableRequestChain GetRecentRequests();
        IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria);
    }
}
