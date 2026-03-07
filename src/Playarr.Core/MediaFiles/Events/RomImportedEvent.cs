using System.Collections.Generic;
using Playarr.Common.Messaging;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.Events
{
    public class EpisodeImportedEvent : IEvent
    {
        public LocalEpisode RomInfo { get; private set; }
        public RomFile ImportedEpisode { get; private set; }
        public List<DeletedRomFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; private set; }

        public EpisodeImportedEvent(LocalEpisode romInfo, RomFile importedEpisode, List<DeletedRomFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            RomInfo = romInfo;
            ImportedEpisode = importedEpisode;
            OldFiles = oldFiles;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClientInfo = downloadClientItem.DownloadClientInfo;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
