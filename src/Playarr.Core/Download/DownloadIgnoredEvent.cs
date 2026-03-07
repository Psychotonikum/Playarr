using System.Collections.Generic;
using Playarr.Common.Messaging;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;

namespace Playarr.Core.Download
{
    public class DownloadIgnoredEvent : IEvent
    {
        public int SeriesId { get; set; }
        public List<int> RomIds { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public string Message { get; set; }
    }
}
