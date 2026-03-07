namespace Playarr.Core.ImportLists
{
    public interface IImportListRequestGenerator
    {
        ImportListPageableRequestChain GetListItems();
    }
}
