using System.Collections.Generic;
using Playarr.Core.Download;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class ImportCompleteMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public List<Rom> Roms { get; set; }
        public List<RomFile> RomFiles { get; set; }
        public string SourcePath { get; set; }
        public string SourceTitle { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public string DestinationPath { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel ReleaseQuality { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
