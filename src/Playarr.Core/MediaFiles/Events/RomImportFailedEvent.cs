using System;
using Playarr.Common.Messaging;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.Events
{
    public class EpisodeImportFailedEvent : IEvent
    {
        public Exception Exception { get; set; }
        public LocalEpisode RomInfo { get; }
        public bool NewDownload { get; }
        public DownloadClientItemClientInfo DownloadClientInfo { get;  }
        public string DownloadId { get; }

        public EpisodeImportFailedEvent(Exception exception, LocalEpisode romInfo, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Exception = exception;
            RomInfo = romInfo;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClientInfo = downloadClientItem.DownloadClientInfo;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
