using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Download;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games.Events;

namespace Playarr.Core.History
{
    public interface IHistoryService
    {
        PagingSpec<EpisodeHistory> Paged(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities);
        EpisodeHistory MostRecentForEpisode(int romId);
        List<EpisodeHistory> FindByRomId(int romId);
        EpisodeHistory MostRecentForDownloadId(string downloadId);
        EpisodeHistory Get(int historyId);
        List<EpisodeHistory> GetBySeries(int gameId, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> GetBySeason(int gameId, int platformNumber, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> GetByEpisode(int romId, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> Find(string downloadId, EpisodeHistoryEventType eventType);
        List<EpisodeHistory> FindByDownloadId(string downloadId);
        string FindDownloadId(EpisodeImportedEvent trackedDownload);
        List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<EpisodeGrabbedEvent>,
                                  IHandle<EpisodeImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<RomFileDeletedEvent>,
                                  IHandle<RomFileRenamedEvent>,
                                  IHandle<SeriesDeletedEvent>,
                                  IHandle<DownloadIgnoredEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public PagingSpec<EpisodeHistory> Paged(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities)
        {
            return _historyRepository.GetPaged(pagingSpec, languages, qualities);
        }

        public EpisodeHistory MostRecentForEpisode(int romId)
        {
            return _historyRepository.MostRecentForEpisode(romId);
        }

        public List<EpisodeHistory> FindByRomId(int romId)
        {
            return _historyRepository.FindByRomId(romId);
        }

        public EpisodeHistory MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public EpisodeHistory Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<EpisodeHistory> GetBySeries(int gameId, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.GetBySeries(gameId, eventType);
        }

        public List<EpisodeHistory> GetBySeason(int gameId, int platformNumber, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.GetBySeason(gameId, platformNumber, eventType);
        }

        public List<EpisodeHistory> GetByEpisode(int romId, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.GetByEpisode(romId, eventType);
        }

        public List<EpisodeHistory> Find(string downloadId, EpisodeHistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<EpisodeHistory> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        public string FindDownloadId(EpisodeImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedEpisode.Path);

            var romIds = trackedDownload.RomInfo.Roms.Select(c => c.Id).ToList();
            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.RomInfo.Game.Id, trackedDownload.ImportedEpisode.Quality);

            // Find download related items for these roms
            var episodesHistory = allHistory.Where(h => romIds.Contains(h.EpisodeId)).ToList();

            var processedDownloadId = episodesHistory
                .Where(c => c.EventType != EpisodeHistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = episodesHistory.Where(c => c.EventType == EpisodeHistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                foreach (var matchingHistory in trackedDownload.RomInfo.Roms.Select(e => stillDownloading.Where(c => c.EpisodeId == e.Id).ToList()))
                {
                    if (matchingHistory.Count != 1)
                    {
                        return null;
                    }

                    var newDownloadId = matchingHistory.Single().DownloadId;

                    if (downloadId == null || downloadId == newDownloadId)
                    {
                        downloadId = newDownloadId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return downloadId;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var rom in message.Rom.Roms)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Rom.ParsedRomInfo.Quality,
                    SourceTitle = message.Rom.Release.Title,
                    SeriesId = rom.SeriesId,
                    EpisodeId = rom.Id,
                    DownloadId = message.DownloadId,
                    Languages = message.Rom.Languages,
                };

                history.Data.Add("Indexer", message.Rom.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Rom.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Rom.ParsedRomInfo.ReleaseGroup);
                history.Data.Add("Age", message.Rom.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Rom.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Rom.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Rom.Release.PublishDate.ToUniversalTime().ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientName", message.DownloadClientName);
                history.Data.Add("Size", message.Rom.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Rom.Release.DownloadUrl);
                history.Data.Add("Guid", message.Rom.Release.Guid);
                history.Data.Add("TvdbId", message.Rom.Release.TvdbId.ToString());
                history.Data.Add("MobyGamesId", message.Rom.Release.MobyGamesId.ToString());
                history.Data.Add("ImdbId", message.Rom.Release.ImdbId);
                history.Data.Add("Protocol", ((int)message.Rom.Release.DownloadProtocol).ToString());
                history.Data.Add("CustomFormatScore", message.Rom.CustomFormatScore.ToString());
                history.Data.Add("SeriesMatchType", message.Rom.SeriesMatchType.ToString());
                history.Data.Add("ReleaseSource", message.Rom.ReleaseSource.ToString());
                history.Data.Add("IndexerFlags", message.Rom.Release.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.Rom.ParsedRomInfo.ReleaseType.ToString());

                if (!message.Rom.ParsedRomInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Rom.ParsedRomInfo.ReleaseHash);
                }

                if (message.Rom.Release is TorrentInfo torrentRelease)
                {
                    history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
                }

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadId = message.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                downloadId = FindDownloadId(message);
            }

            foreach (var rom in message.RomInfo.Roms)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.DownloadFolderImported,
                    Date = DateTime.UtcNow,
                    Quality = message.RomInfo.Quality,
                    SourceTitle = message.ImportedEpisode.SceneName ?? Path.GetFileNameWithoutExtension(message.RomInfo.Path),
                    SeriesId = message.ImportedEpisode.SeriesId,
                    EpisodeId = rom.Id,
                    DownloadId = downloadId,
                    Languages = message.RomInfo.Languages
                };

                history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.RomInfo.Path);
                history.Data.Add("ImportedPath", Path.Combine(message.RomInfo.Game.Path, message.ImportedEpisode.RelativePath));
                history.Data.Add("DownloadClient", message.DownloadClientInfo?.Type);
                history.Data.Add("DownloadClientName", message.DownloadClientInfo?.Name);
                history.Data.Add("ReleaseGroup", message.RomInfo.ReleaseGroup);
                history.Data.Add("CustomFormatScore", message.RomInfo.CustomFormatScore.ToString());
                history.Data.Add("Size", message.RomInfo.Size.ToString());
                history.Data.Add("IndexerFlags", message.ImportedEpisode.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.ImportedEpisode.ReleaseType.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadFailedEvent message)
        {
            foreach (var romId in message.RomIds)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    SeriesId = message.SeriesId,
                    EpisodeId = romId,
                    DownloadId = message.DownloadId,
                    Languages = message.Languages
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientName", message.TrackedDownload?.DownloadItem.DownloadClientInfo.Name);
                history.Data.Add("Message", message.Message);
                history.Data.Add("Source", message.Source);
                history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteEpisode?.ParsedRomInfo?.ReleaseGroup ?? message.Data.GetValueOrDefault(EpisodeHistory.RELEASE_GROUP));
                history.Data.Add("Size", message.TrackedDownload?.DownloadItem.TotalSize.ToString() ?? message.Data.GetValueOrDefault(EpisodeHistory.SIZE));
                history.Data.Add("Indexer", message.TrackedDownload?.RemoteEpisode?.Release?.Indexer ?? message.Data.GetValueOrDefault(EpisodeHistory.INDEXER));

                _historyRepository.Insert(history);
            }
        }

        public void Handle(RomFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing rom file from DB as part of cleanup routine, not creating history event.");
                return;
            }
            else if (message.Reason == DeleteMediaFileReason.ManualOverride)
            {
                _logger.Debug("Removing rom file from DB as part of manual override of existing file, not creating history event.");
                return;
            }

            foreach (var rom in message.RomFile.Roms.Value)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.RomFileDeleted,
                    Date = DateTime.UtcNow,
                    Quality = message.RomFile.Quality,
                    SourceTitle = message.RomFile.Path,
                    SeriesId = message.RomFile.SeriesId,
                    EpisodeId = rom.Id,
                    Languages = message.RomFile.Languages
                };

                history.Data.Add("Reason", message.Reason.ToString());
                history.Data.Add("ReleaseGroup", message.RomFile.ReleaseGroup);
                history.Data.Add("Size", message.RomFile.Size.ToString());
                history.Data.Add("IndexerFlags", message.RomFile.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.RomFile.ReleaseType.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(RomFileRenamedEvent message)
        {
            var sourcePath = message.OriginalPath;
            var sourceRelativePath = message.Game.Path.GetRelativePath(message.OriginalPath);
            var path = Path.Combine(message.Game.Path, message.RomFile.RelativePath);
            var relativePath = message.RomFile.RelativePath;

            foreach (var rom in message.RomFile.Roms.Value)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.RomFileRenamed,
                    Date = DateTime.UtcNow,
                    Quality = message.RomFile.Quality,
                    SourceTitle = message.OriginalPath,
                    SeriesId = message.RomFile.SeriesId,
                    EpisodeId = rom.Id,
                    Languages = message.RomFile.Languages
                };

                history.Data.Add("SourcePath", sourcePath);
                history.Data.Add("SourceRelativePath", sourceRelativePath);
                history.Data.Add("Path", path);
                history.Data.Add("RelativePath", relativePath);
                history.Data.Add("ReleaseGroup", message.RomFile.ReleaseGroup);
                history.Data.Add("Size", message.RomFile.Size.ToString());
                history.Data.Add("IndexerFlags", message.RomFile.IndexerFlags.ToString());
                history.Data.Add("ReleaseType", message.RomFile.ReleaseType.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadIgnoredEvent message)
        {
            var historyToAdd = new List<EpisodeHistory>();

            foreach (var romId in message.RomIds)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.DownloadIgnored,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    SeriesId = message.SeriesId,
                    EpisodeId = romId,
                    DownloadId = message.DownloadId,
                    Languages = message.Languages
                };

                history.Data.Add("DownloadClient", message.DownloadClientInfo.Type);
                history.Data.Add("DownloadClientName", message.DownloadClientInfo.Name);
                history.Data.Add("Message", message.Message);
                history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteEpisode?.ParsedRomInfo?.ReleaseGroup);
                history.Data.Add("Size", message.TrackedDownload?.DownloadItem.TotalSize.ToString());
                history.Data.Add("Indexer", message.TrackedDownload?.RemoteEpisode?.Release?.Indexer);
                history.Data.Add("ReleaseType", message.TrackedDownload?.RemoteEpisode?.ParsedRomInfo?.ReleaseType.ToString());

                historyToAdd.Add(history);
            }

            _historyRepository.InsertMany(historyToAdd);
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _historyRepository.DeleteForSeries(message.Game.Select(m => m.Id).ToList());
        }

        public List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.Since(date, eventType);
        }
    }
}
