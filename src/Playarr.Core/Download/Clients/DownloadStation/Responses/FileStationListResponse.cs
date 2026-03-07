using System.Collections.Generic;

namespace Playarr.Core.Download.Clients.DownloadStation.Responses
{
    public class FileStationListResponse
    {
        public List<FileStationListFileInfoResponse> Files { get; set; }
    }
}
