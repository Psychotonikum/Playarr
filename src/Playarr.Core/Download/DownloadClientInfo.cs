using System.Collections.Generic;
using Playarr.Common.Disk;

namespace Playarr.Core.Download
{
    public class DownloadClientInfo
    {
        public DownloadClientInfo()
        {
            OutputRootFolders = new List<OsPath>();
        }

        public bool IsLocalhost { get; set; }
        public string SortingMode { get; set; }
        public bool RemovesCompletedDownloads { get; set; }
        public List<OsPath> OutputRootFolders { get; set; }
    }
}
