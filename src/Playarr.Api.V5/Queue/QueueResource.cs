using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Core.Queue;
using Playarr.Api.V5.CustomFormats;
using Playarr.Api.V5.Roms;
using Playarr.Api.V5.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Queue
{
    public class QueueResource : RestResource
    {
        public int? SeriesId { get; set; }
        public IEnumerable<int> RomIds { get; set; } = [];
        public List<int> PlatformNumbers { get; set; } = [];
        public GameResource? Game { get; set; }
        public List<RomResource>? Roms { get; set; }
        public List<Language> Languages { get; set; } = [];
        public QualityModel Quality { get; set; } = new(Playarr.Core.Qualities.Quality.Unknown);
        public List<CustomFormatResource> CustomFormats { get; set; } = [];
        public int CustomFormatScore { get; set; }
        public decimal Size { get; set; }
        public string? Title { get; set; }
        public decimal SizeLeft { get; set; }
        public TimeSpan? TimeLeft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage>? StatusMessages { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string? DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string? Indexer { get; set; }
        public string? OutputPath { get; set; }
        public int EpisodesWithFilesCount { get; set; }
        public bool IsFullSeason { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this Playarr.Core.Queue.Queue model, bool includeSeries, bool includeEpisodes)
        {
            var customFormats = model.RemoteEpisode?.CustomFormats;
            var customFormatScore = model.Game?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new QueueResource
            {
                Id = model.Id,
                SeriesId = model.Game?.Id,
                RomIds = model.Roms?.Select(e => e.Id).ToList() ?? [],
                PlatformNumbers = model.SeasonNumber.HasValue ? [model.SeasonNumber.Value] : [],
                Game = includeSeries && model.Game != null ? model.Game.ToResource() : null,
                Roms = includeEpisodes ? model.Roms?.ToResource() : null,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats?.ToResource(false) ?? [],
                CustomFormatScore = customFormatScore,
                Size = model.Size,
                Title = model.Title,
                SizeLeft = model.SizeLeft,
                TimeLeft = model.TimeLeft,
                EstimatedCompletionTime = model.EstimatedCompletionTime,
                Added = model.Added,
                Status = model.Status,
                TrackedDownloadStatus = model.TrackedDownloadStatus,
                TrackedDownloadState = model.TrackedDownloadState,
                StatusMessages = model.StatusMessages,
                ErrorMessage = model.ErrorMessage,
                DownloadId = model.DownloadId,
                Protocol = model.Protocol,
                DownloadClient = model.DownloadClient,
                DownloadClientHasPostImportCategory = model.DownloadClientHasPostImportCategory,
                Indexer = model.Indexer,
                OutputPath = model.OutputPath,
                EpisodesWithFilesCount = model.Roms?.Count(e => e.HasFile) ?? 0,
                IsFullSeason = model.RemoteEpisode?.ParsedRomInfo?.FullSeason ?? false
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<Playarr.Core.Queue.Queue> models, bool includeSeries, bool includeEpisode)
        {
            return models.Select((m) => ToResource(m, includeSeries, includeEpisode)).ToList();
        }
    }
}
