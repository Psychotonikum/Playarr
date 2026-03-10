using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.HealthCheck;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Qualities;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;
using Playarr.Core.Update.History.Events;

namespace Playarr.Core.Notifications
{
    public class NotificationService
        : IHandle<EpisodeGrabbedEvent>,
          IHandle<EpisodeImportedEvent>,
          IHandle<DownloadCompletedEvent>,
          IHandle<UntrackedDownloadCompletedEvent>,
          IHandle<SeriesRenamedEvent>,
          IHandle<SeriesAddCompletedEvent>,
          IHandle<SeriesDeletedEvent>,
          IHandle<RomFileDeletedEvent>,
          IHandle<HealthCheckFailedEvent>,
          IHandle<HealthCheckRestoredEvent>,
          IHandle<UpdateInstalledEvent>,
          IHandle<ManualInteractionRequiredEvent>,
          IHandleAsync<DeleteCompletedEvent>,
          IHandleAsync<DownloadsProcessedEvent>,
          IHandleAsync<RenameCompletedEvent>,
          IHandleAsync<HealthCheckCompleteEvent>
    {
        private readonly INotificationFactory _notificationFactory;
        private readonly INotificationStatusService _notificationStatusService;
        private readonly Logger _logger;

        public NotificationService(INotificationFactory notificationFactory, INotificationStatusService notificationStatusService, Logger logger)
        {
            _notificationFactory = notificationFactory;
            _notificationStatusService = notificationStatusService;
            _logger = logger;
        }

        private string GetMessage(Game game, List<Rom> roms, QualityModel quality)
        {
            var qualityString = GetQualityString(game, quality);

            if (roms.Empty())
            {
                return $"{game.Title} - [{qualityString}]";
            }

            var romTitles = string.Join(" + ", roms.Select(e => e.Title));

            return $"{game.Title} - {romTitles} [{qualityString}]";
        }

        private string GetFullSeasonMessage(Game game, int platformNumber, QualityModel quality)
        {
            var qualityString = GetQualityString(game, quality);

            return $"{game.Title} - Platform {platformNumber} [{qualityString}]";
        }

        private string GetQualityString(Game game, QualityModel quality)
        {
            var qualityString = quality.Quality.ToString();

            if (quality.Revision.Version > 1)
            {
                qualityString += " Proper";
            }

            return qualityString;
        }

        private bool ShouldHandleSeries(ProviderDefinition definition, Game game)
        {
            if (definition.Tags.Empty())
            {
                _logger.Debug("No tags set for this notification.");
                return true;
            }

            if (definition.Tags.Intersect(game.Tags).Any())
            {
                _logger.Debug("Notification and game have one or more intersecting tags.");
                return true;
            }

            _logger.Debug("{0} does not have any intersecting tags with {1}. Notification will not be sent.", definition.Name, game.Title);
            return false;
        }

        private bool ShouldHandleHealthFailure(HealthCheck.HealthCheck healthCheck, bool includeWarnings)
        {
            if (healthCheck.Type == HealthCheckResult.Error)
            {
                return true;
            }

            if (healthCheck.Type == HealthCheckResult.Warning && includeWarnings)
            {
                return true;
            }

            return false;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            var grabMessage = new GrabMessage
            {
                Message = GetMessage(message.Rom.Game, message.Rom.Roms, message.Rom.ParsedRomInfo.Quality),
                Game = message.Rom.Game,
                Quality = message.Rom.ParsedRomInfo.Quality,
                Rom = message.Rom,
                DownloadClientType = message.DownloadClient,
                DownloadClientName = message.DownloadClientName,
                DownloadId = message.DownloadId
            };

            foreach (var notification in _notificationFactory.OnGrabEnabled())
            {
                try
                {
                    if (!ShouldHandleSeries(notification.Definition, message.Rom.Game))
                    {
                        continue;
                    }

                    notification.OnGrab(grabMessage);
                    _notificationStatusService.RecordSuccess(notification.Definition.Id);
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Error(ex, "Unable to send OnGrab notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadMessage = new DownloadMessage
            {
                Message = GetMessage(message.RomInfo.Game, message.RomInfo.Roms, message.RomInfo.Quality),
                Game = message.RomInfo.Game,
                RomInfo = message.RomInfo,
                RomFile = message.ImportedEpisode,
                OldFiles = message.OldFiles,
                SourcePath = message.RomInfo.Path,
                DownloadClientInfo = message.DownloadClientInfo,
                DownloadId = message.DownloadId,
                Release = message.RomInfo.Release
            };

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, message.RomInfo.Game))
                    {
                        if (downloadMessage.OldFiles.Empty() || ((NotificationDefinition)notification.Definition).OnUpgrade)
                        {
                            notification.OnDownload(downloadMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnDownload notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(DownloadCompletedEvent message)
        {
            var game = message.TrackedDownload.RemoteRom.Game;
            var roms = message.TrackedDownload.RemoteRom.Roms;
            var parsedRomInfo = message.TrackedDownload.RemoteRom.ParsedRomInfo;

            var downloadMessage = new ImportCompleteMessage
            {
                Message = parsedRomInfo.FullSeason
                    ? GetFullSeasonMessage(game, roms.First().PlatformNumber, parsedRomInfo.Quality)
                    : GetMessage(game, roms, parsedRomInfo.Quality),
                Game = game,
                Roms = roms,
                RomFiles = message.RomFiles,
                DownloadClientInfo = message.TrackedDownload.DownloadItem.DownloadClientInfo,
                DownloadId = message.TrackedDownload.DownloadItem.DownloadId,
                Release = message.Release,
                SourcePath = message.TrackedDownload.DownloadItem.OutputPath.FullPath,
                DestinationPath = message.RomFiles.Select(e => Path.Join(game.Path, e.RelativePath)).ToList().GetLongestCommonPath(),
                ReleaseGroup = parsedRomInfo.ReleaseGroup,
                ReleaseQuality = parsedRomInfo.Quality
            };

            foreach (var notification in _notificationFactory.OnImportCompleteEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, game))
                    {
                        if (((NotificationDefinition)notification.Definition).OnImportComplete)
                        {
                            notification.OnImportComplete(downloadMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnImportComplete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(UntrackedDownloadCompletedEvent message)
        {
            var game = message.Game;
            var roms = message.Roms;
            var parsedRomInfo = message.ParsedRomInfo;

            var downloadMessage = new ImportCompleteMessage
            {
                Message = parsedRomInfo.FullSeason
                    ? GetFullSeasonMessage(game, roms.First().PlatformNumber, parsedRomInfo.Quality)
                    : GetMessage(game, roms, parsedRomInfo.Quality),
                Game = game,
                Roms = roms,
                RomFiles = message.RomFiles,
                SourcePath = message.SourcePath,
                SourceTitle = parsedRomInfo.ReleaseTitle,
                DestinationPath = message.RomFiles.Select(e => Path.Join(game.Path, e.RelativePath)).ToList().GetLongestCommonPath(),
                ReleaseGroup = parsedRomInfo.ReleaseGroup,
                ReleaseQuality = parsedRomInfo.Quality
            };

            foreach (var notification in _notificationFactory.OnImportCompleteEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, game))
                    {
                        if (((NotificationDefinition)notification.Definition).OnImportComplete)
                        {
                            notification.OnImportComplete(downloadMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnImportComplete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            foreach (var notification in _notificationFactory.OnRenameEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, message.Game))
                    {
                        notification.OnRename(message.Game, message.RenamedFiles);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnRename notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(UpdateInstalledEvent message)
        {
            var updateMessage = new ApplicationUpdateMessage();
            updateMessage.Message = $"Playarr updated from {message.PreviousVerison.ToString()} to {message.NewVersion.ToString()}";
            updateMessage.PreviousVersion = message.PreviousVerison;
            updateMessage.NewVersion = message.NewVersion;

            foreach (var notification in _notificationFactory.OnApplicationUpdateEnabled())
            {
                try
                {
                    notification.OnApplicationUpdate(updateMessage);
                    _notificationStatusService.RecordSuccess(notification.Definition.Id);
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnApplicationUpdate notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(ManualInteractionRequiredEvent message)
        {
            var game = message.Rom?.Game;
            var mess = "";

            if (game != null)
            {
                mess = GetMessage(game, message.Rom.Roms, message.Rom.ParsedRomInfo.Quality);
            }

            if (mess.IsNullOrWhiteSpace() && message.TrackedDownload.DownloadItem != null)
            {
                mess = message.TrackedDownload.DownloadItem.Title;
            }

            if (mess.IsNullOrWhiteSpace())
            {
                return;
            }

            var manualInteractionMessage = new ManualInteractionRequiredMessage
            {
                Message = mess,
                Game = game,
                Quality = message.Rom?.ParsedRomInfo.Quality,
                Rom = message.Rom,
                TrackedDownload = message.TrackedDownload,
                DownloadClientInfo = message.TrackedDownload.DownloadItem?.DownloadClientInfo,
                DownloadId = message.TrackedDownload.DownloadItem?.DownloadId,
                Release = message.Release
            };

            foreach (var notification in _notificationFactory.OnManualInteractionEnabled())
            {
                try
                {
                    if (!ShouldHandleSeries(notification.Definition, message.Rom.Game))
                    {
                        continue;
                    }

                    notification.OnManualInteractionRequired(manualInteractionMessage);
                    _notificationStatusService.RecordSuccess(notification.Definition.Id);
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Error(ex, "Unable to send OnManualInteractionRequired notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(RomFileDeletedEvent message)
        {
            if (message.RomFile.Roms.Value.Empty())
            {
                _logger.Trace("Skipping notification for deleted file without an rom (rom metadata was removed)");

                return;
            }

            var deleteMessage = new EpisodeDeleteMessage();
            deleteMessage.Message = GetMessage(message.RomFile.Game, message.RomFile.Roms, message.RomFile.Quality);
            deleteMessage.Game = message.RomFile.Game;
            deleteMessage.RomFile = message.RomFile;
            deleteMessage.Reason = message.Reason;

            foreach (var notification in _notificationFactory.OnRomFileDeleteEnabled())
            {
                try
                {
                    if (message.Reason != MediaFiles.DeleteMediaFileReason.Upgrade || ((NotificationDefinition)notification.Definition).OnRomFileDeleteForUpgrade)
                    {
                        if (ShouldHandleSeries(notification.Definition, deleteMessage.RomFile.Game))
                        {
                            notification.OnRomFileDelete(deleteMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnRomFileDelete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(SeriesAddCompletedEvent message)
        {
            var game = message.Game;
            var addMessage = new SeriesAddMessage
            {
                Game = game,
                Message = game.Title
            };

            foreach (var notification in _notificationFactory.OnSeriesAddEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, game))
                    {
                        notification.OnSeriesAdd(addMessage);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnSeriesAdd notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(SeriesDeletedEvent message)
        {
            foreach (var game in message.Game)
            {
                var deleteMessage = new SeriesDeleteMessage(game, message.DeleteFiles);

                foreach (var notification in _notificationFactory.OnSeriesDeleteEnabled())
                {
                    try
                    {
                        if (ShouldHandleSeries(notification.Definition, deleteMessage.Game))
                        {
                            notification.OnSeriesDelete(deleteMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationStatusService.RecordFailure(notification.Definition.Id);
                        _logger.Warn(ex, "Unable to send OnSeriesDelete notification to: " + notification.Definition.Name);
                    }
                }
            }
        }

        public void Handle(HealthCheckFailedEvent message)
        {
            // Don't send health check notifications during the start up grace period,
            // once that duration expires they they'll be retested and fired off if necessary.

            if (message.IsInStartupGracePeriod)
            {
                return;
            }

            foreach (var notification in _notificationFactory.OnHealthIssueEnabled())
            {
                try
                {
                    if (ShouldHandleHealthFailure(message.HealthCheck, ((NotificationDefinition)notification.Definition).IncludeHealthWarnings))
                    {
                        notification.OnHealthIssue(message.HealthCheck);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnHealthIssue notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(HealthCheckRestoredEvent message)
        {
            if (message.IsInStartupGracePeriod)
            {
                return;
            }

            foreach (var notification in _notificationFactory.OnHealthRestoredEnabled())
            {
                try
                {
                    if (ShouldHandleHealthFailure(message.PreviousCheck, ((NotificationDefinition)notification.Definition).IncludeHealthWarnings))
                    {
                        notification.OnHealthRestored(message.PreviousCheck);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnHealthRestored notification to: " + notification.Definition.Name);
                }
            }
        }

        public void HandleAsync(DeleteCompletedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(DownloadsProcessedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(RenameCompletedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(HealthCheckCompleteEvent message)
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            foreach (var notification in _notificationFactory.GetAvailableProviders())
            {
                try
                {
                    notification.ProcessQueue();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to process notification queue for " + notification.Definition.Name);
                }
            }
        }
    }
}
