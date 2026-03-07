using System.Collections.Generic;
using Playarr.Core.Download;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class DownloadMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public LocalEpisode RomInfo { get; set; }
        public RomFile RomFile { get; set; }
        public List<DeletedRomFile> OldFiles { get; set; }
        public string SourcePath { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public GrabbedReleaseInfo Release { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
