using System.Collections.Generic;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists
{
    public interface IParseImportListResponse
    {
        IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse);
    }
}
