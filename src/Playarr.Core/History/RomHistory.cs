using System;
using System.Collections.Generic;
using Playarr.Core.Datastore;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.History
{
    public class EpisodeHistory : ModelBase
    {
        public const string DOWNLOAD_CLIENT = "downloadClient";
        public const string SERIES_MATCH_TYPE = "seriesMatchType";
        public const string RELEASE_SOURCE = "releaseSource";
        public const string RELEASE_GROUP = "releaseGroup";
        public const string SIZE = "size";
        public const string INDEXER = "indexer";

        public EpisodeHistory()
        {
            Data = new Dictionary<string, string>();
        }

        public int EpisodeId { get; set; }
        public int GameId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public Rom Rom { get; set; }
        public Game Game { get; set; }
        public EpisodeHistoryEventType EventType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<Language> Languages { get; set; }
        public string DownloadId { get; set; }
    }

    public enum EpisodeHistoryEventType
    {
        Unknown = 0,
        Grabbed = 1,
        GameFolderImported = 2,
        DownloadFolderImported = 3,
        DownloadFailed = 4,
        RomFileDeleted = 5,
        RomFileRenamed = 6,
        DownloadIgnored = 7
    }
}
