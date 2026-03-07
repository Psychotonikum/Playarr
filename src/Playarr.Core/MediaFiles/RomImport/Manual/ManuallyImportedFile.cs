using Playarr.Core.Download.TrackedDownloads;

namespace Playarr.Core.MediaFiles.EpisodeImport.Manual
{
    public class ManuallyImportedFile
    {
        public TrackedDownload TrackedDownload { get; set; }
        public ImportResult ImportResult { get; set; }
    }
}
