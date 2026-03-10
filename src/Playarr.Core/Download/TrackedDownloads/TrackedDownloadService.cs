using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Cache;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.Download.Aggregation;
using Playarr.Core.Download.History;
using Playarr.Core.History;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService
    {
        TrackedDownload Find(string downloadId);
        void StopTracking(string downloadId);
        void StopTracking(List<string> downloadIds);
        TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem);
        List<TrackedDownload> GetTrackedDownloads();
        void UpdateTrackable(List<TrackedDownload> trackedDownloads);
    }

    public class TrackedDownloadService : ITrackedDownloadService,
                                          IHandle<RomInfoRefreshedEvent>,
                                          IHandle<SeriesAddedEvent>,
                                          IHandle<SeriesEditedEvent>,
                                          IHandle<SeriesBulkEditedEvent>,
                                          IHandle<SeriesDeletedEvent>
    {
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDownloadHistoryService _downloadHistoryService;
        private readonly IRemoteEpisodeAggregationService _aggregationService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
                                      ICacheManager cacheManager,
                                      IHistoryService historyService,
                                      ICustomFormatCalculationService formatCalculator,
                                      IEventAggregator eventAggregator,
                                      IDownloadHistoryService downloadHistoryService,
                                      IRemoteEpisodeAggregationService aggregationService,
                                      Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _formatCalculator = formatCalculator;
            _eventAggregator = eventAggregator;
            _downloadHistoryService = downloadHistoryService;
            _aggregationService = aggregationService;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _logger = logger;
        }

        public TrackedDownload Find(string downloadId)
        {
            return _cache.Find(downloadId);
        }

        public void StopTracking(string downloadId)
        {
            var trackedDownload = _cache.Find(downloadId);

            _cache.Remove(downloadId);
            _eventAggregator.PublishEvent(new TrackedDownloadsRemovedEvent(new List<TrackedDownload> { trackedDownload }));
        }

        public void StopTracking(List<string> downloadIds)
        {
            var trackedDownloads = new List<TrackedDownload>();

            foreach (var downloadId in downloadIds)
            {
                var trackedDownload = _cache.Find(downloadId);
                _cache.Remove(downloadId);
                trackedDownloads.Add(trackedDownload);
            }

            _eventAggregator.PublishEvent(new TrackedDownloadsRemovedEvent(trackedDownloads));
        }

        public TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem)
        {
            var existingItem = Find(downloadItem.DownloadId);

            if (existingItem != null && existingItem.State != TrackedDownloadState.Downloading)
            {
                LogItemChange(existingItem, existingItem.DownloadItem, downloadItem);

                existingItem.DownloadItem = downloadItem;
                existingItem.IsTrackable = true;

                return existingItem;
            }

            var trackedDownload = new TrackedDownload
            {
                DownloadClient = downloadClient.Id,
                DownloadItem = downloadItem,
                Protocol = downloadClient.Protocol,
                IsTrackable = true,
                HasNotifiedManualInteractionRequired = existingItem?.HasNotifiedManualInteractionRequired ?? false
            };

            try
            {
                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                var parsedRomInfo = Parser.Parser.ParseTitle(trackedDownload.DownloadItem.Title);

                if (parsedRomInfo != null)
                {
                    trackedDownload.RemoteRom = _parsingService.Map(parsedRomInfo, 0, 0, null);
                }

                var downloadHistory = _downloadHistoryService.GetLatestDownloadHistoryItem(downloadItem.DownloadId);

                if (downloadHistory != null)
                {
                    var state = GetStateFromHistory(downloadHistory.EventType);
                    trackedDownload.State = state;
                }

                if (historyItems.Any())
                {
                    var firstHistoryItem = historyItems.First();
                    var grabbedEvent = historyItems.FirstOrDefault(v => v.EventType == EpisodeHistoryEventType.Grabbed);

                    trackedDownload.Indexer = grabbedEvent?.Data?.GetValueOrDefault("indexer");
                    trackedDownload.Added = grabbedEvent?.Date;

                    if (parsedRomInfo == null ||
                        trackedDownload.RemoteRom?.Game == null ||
                        trackedDownload.RemoteRom.Roms.Empty())
                    {
                        // Try parsing the original source title and if that fails, try parsing it as a special
                        // TODO: Pass the IGDB ID and TVRage IDs in as well so we have a better chance for finding the item
                        parsedRomInfo = Parser.Parser.ParseTitle(firstHistoryItem.SourceTitle) ??
                                            _parsingService.ParseSpecialRomTitle(parsedRomInfo, firstHistoryItem.SourceTitle, 0, 0, null);

                        if (parsedRomInfo != null)
                        {
                            trackedDownload.RemoteRom = _parsingService.Map(parsedRomInfo,
                                firstHistoryItem.GameId,
                                historyItems.Where(v => v.EventType == EpisodeHistoryEventType.Grabbed)
                                    .Select(h => h.EpisodeId).Distinct());
                        }
                    }

                    if (trackedDownload.RemoteRom != null)
                    {
                        trackedDownload.RemoteRom.Release ??= new ReleaseInfo();
                        trackedDownload.RemoteRom.Release.Indexer = trackedDownload.Indexer;
                        trackedDownload.RemoteRom.Release.Title = trackedDownload.RemoteRom.ParsedRomInfo?.ReleaseTitle;

                        if (Enum.TryParse(grabbedEvent?.Data?.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags))
                        {
                            trackedDownload.RemoteRom.Release.IndexerFlags = flags;
                        }

                        if (downloadHistory != null)
                        {
                            trackedDownload.RemoteRom.Release.IndexerId = downloadHistory.IndexerId;
                        }
                    }
                }

                if (trackedDownload.RemoteRom != null)
                {
                    _aggregationService.Augment(trackedDownload.RemoteRom);

                    // Calculate custom formats
                    trackedDownload.RemoteRom.CustomFormats = _formatCalculator.ParseCustomFormat(trackedDownload.RemoteRom, downloadItem.TotalSize);
                }

                // Track it so it can be displayed in the queue even though we can't determine which game it is for
                if (trackedDownload.RemoteRom == null)
                {
                    _logger.Trace("No Rom found for download '{0}'", trackedDownload.DownloadItem.Title);
                }
            }
            catch (MultipleSeriesFoundException e)
            {
                _logger.Debug(e, "Found multiple game for " + downloadItem.Title);

                trackedDownload.Warn("Unable to import automatically, found multiple game: {0}", string.Join(", ", e.Game));
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed to find rom for " + downloadItem.Title);

                trackedDownload.Warn("Unable to parse roms from title");
            }

            LogItemChange(trackedDownload, existingItem?.DownloadItem, trackedDownload.DownloadItem);

            _cache.Set(trackedDownload.DownloadItem.DownloadId, trackedDownload);
            return trackedDownload;
        }

        public List<TrackedDownload> GetTrackedDownloads()
        {
            return _cache.Values.ToList();
        }

        public void UpdateTrackable(List<TrackedDownload> trackedDownloads)
        {
            var untrackable = GetTrackedDownloads().ExceptBy(t => t.DownloadItem.DownloadId, trackedDownloads, t => t.DownloadItem.DownloadId, StringComparer.CurrentCulture).ToList();

            foreach (var trackedDownload in untrackable)
            {
                trackedDownload.IsTrackable = false;
            }
        }

        private void LogItemChange(TrackedDownload trackedDownload, DownloadClientItem existingItem, DownloadClientItem downloadItem)
        {
            if (existingItem == null ||
                existingItem.Status != downloadItem.Status ||
                existingItem.CanBeRemoved != downloadItem.CanBeRemoved ||
                existingItem.CanMoveFiles != downloadItem.CanMoveFiles)
            {
                _logger.Debug("Tracking '{0}:{1}': ClientState={2}{3} PlayarrStage={4} Rom='{5}' OutputPath={6}.",
                    downloadItem.DownloadClientInfo.Name,
                    downloadItem.Title,
                    downloadItem.Status,
                    downloadItem.CanBeRemoved ? "" : downloadItem.CanMoveFiles ? " (busy)" : " (readonly)",
                    trackedDownload.State,
                    trackedDownload.RemoteRom?.ParsedRomInfo,
                    downloadItem.OutputPath);
            }
        }

        private void UpdateCachedItem(TrackedDownload trackedDownload)
        {
            var parsedRomInfo = Parser.Parser.ParseTitle(trackedDownload.DownloadItem.Title);

            trackedDownload.RemoteRom = parsedRomInfo == null ? null : _parsingService.Map(parsedRomInfo, 0, 0, null);

            _aggregationService.Augment(trackedDownload.RemoteRom);
        }

        private static TrackedDownloadState GetStateFromHistory(DownloadHistoryEventType eventType)
        {
            switch (eventType)
            {
                case DownloadHistoryEventType.DownloadImported:
                    return TrackedDownloadState.Imported;
                case DownloadHistoryEventType.DownloadFailed:
                    return TrackedDownloadState.Failed;
                case DownloadHistoryEventType.DownloadIgnored:
                    return TrackedDownloadState.Ignored;
                default:
                    return TrackedDownloadState.Downloading;
            }
        }

        public void Handle(RomInfoRefreshedEvent message)
        {
            var needsToUpdate = false;

            foreach (var rom in message.Removed)
            {
                var cachedItems = _cache.Values.Where(t =>
                                            t.RemoteRom?.Roms != null &&
                                            t.RemoteRom.Roms.Any(e => e.Id == rom.Id))
                                        .ToList();

                if (cachedItems.Any())
                {
                    needsToUpdate = true;
                }

                cachedItems.ForEach(UpdateCachedItem);
            }

            if (needsToUpdate)
            {
                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }

        public void Handle(SeriesAddedEvent message)
        {
            var cachedItems = _cache.Values
                .Where(t =>
                    t.RemoteRom?.Game == null ||
                    message.Game?.IgdbId == t.RemoteRom.Game.IgdbId)
                .ToList();

            if (cachedItems.Any())
            {
                cachedItems.ForEach(UpdateCachedItem);

                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }

        public void Handle(SeriesEditedEvent message)
        {
            var cachedItems = _cache.Values
                .Where(t =>
                    t.RemoteRom?.Game != null &&
                    (t.RemoteRom.Game.Id == message.Game?.Id || t.RemoteRom.Game.IgdbId == message.Game?.IgdbId))
                .ToList();

            if (cachedItems.Any())
            {
                cachedItems.ForEach(UpdateCachedItem);

                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }

        public void Handle(SeriesBulkEditedEvent message)
        {
            var cachedItems = _cache.Values
                .Where(t =>
                    t.RemoteRom?.Game != null &&
                    message.Game.Any(s => s.Id == t.RemoteRom.Game.Id || s.IgdbId == t.RemoteRom.Game.IgdbId))
                .ToList();

            if (cachedItems.Any())
            {
                cachedItems.ForEach(UpdateCachedItem);

                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }

        public void Handle(SeriesDeletedEvent message)
        {
            var cachedItems = _cache.Values
                .Where(t =>
                    t.RemoteRom?.Game != null &&
                    message.Game.Any(s => s.Id == t.RemoteRom.Game.Id || s.IgdbId == t.RemoteRom.Game.IgdbId))
                .ToList();

            if (cachedItems.Any())
            {
                cachedItems.ForEach(UpdateCachedItem);

                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }
    }
}
