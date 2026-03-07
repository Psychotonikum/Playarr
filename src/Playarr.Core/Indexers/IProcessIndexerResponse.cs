using System.Collections.Generic;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Indexers
{
    public interface IParseIndexerResponse
    {
        IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse);
    }
}
