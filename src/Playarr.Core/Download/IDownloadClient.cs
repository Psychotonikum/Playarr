using System.Collections.Generic;
using System.Threading.Tasks;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Download
{
    public interface IDownloadClient : IProvider
    {
        DownloadProtocol Protocol { get; }
        Task<string> Download(RemoteRom remoteRom, IIndexer indexer);
        IEnumerable<DownloadClientItem> GetItems();
        DownloadClientItem GetImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt);
        void RemoveItem(DownloadClientItem item, bool deleteData);
        DownloadClientInfo GetStatus();
        void MarkItemAsImported(DownloadClientItem downloadClientItem);
    }
}
