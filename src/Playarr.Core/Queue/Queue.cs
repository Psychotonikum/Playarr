using System;
using System.Collections.Generic;
using Playarr.Core.Datastore;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Queue
{
    public class Queue : ModelBase
    {
        public Game Game { get; set; }

        public int? SeasonNumber { get; set; }

        [Obsolete]
        public Rom Rom { get; set; }

        public List<Rom> Roms { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }
        public decimal SizeLeft { get; set; }
        public TimeSpan? TimeLeft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string DownloadId { get; set; }
        public RemoteEpisode RemoteEpisode { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }
        public string ErrorMessage { get; set; }
    }
}
