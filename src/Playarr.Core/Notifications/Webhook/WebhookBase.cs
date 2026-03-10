using System.Collections.Generic;
using System.IO;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.Tags;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Webhook
{
    public abstract class WebhookBase<TSettings> : NotificationBase<TSettings>
        where TSettings : NotificationSettingsBase<TSettings>, new()
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        protected readonly ILocalizationService _localizationService;
        private readonly ITagRepository _tagRepository;
        private readonly IMapCoversToLocal _mediaCoverService;

        protected WebhookBase(IConfigFileProvider configFileProvider, IConfigService configService, ILocalizationService localizationService, ITagRepository tagRepository, IMapCoversToLocal mediaCoverService)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _localizationService = localizationService;
            _tagRepository = tagRepository;
            _mediaCoverService = mediaCoverService;
        }

        protected WebhookGrabPayload BuildOnGrabPayload(GrabMessage message)
        {
            var remoteRom = message.Rom;
            var quality = message.Quality;

            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Grab,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(message.Game),
                Roms = remoteRom.Roms.ConvertAll(x => new WebhookEpisode(x)),
                Release = new WebhookRelease(quality, remoteRom),
                DownloadClient = message.DownloadClientName,
                DownloadClientType = message.DownloadClientType,
                DownloadId = message.DownloadId,
                CustomFormatInfo = new WebhookCustomFormatInfo(remoteRom.CustomFormats, remoteRom.CustomFormatScore),
            };
        }

        protected WebhookImportPayload BuildOnDownloadPayload(DownloadMessage message)
        {
            var romFile = message.RomFile;

            var payload = new WebhookImportPayload
            {
                EventType = WebhookEventType.Download,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(message.Game),
                Roms = romFile.Roms.Value.ConvertAll(x => new WebhookEpisode(x)),
                RomFile = new WebhookRomFile(romFile)
                {
                    SourcePath = message.SourcePath
                },
                Release = new WebhookGrabbedRelease(message.Release, romFile.IndexerFlags, romFile.ReleaseType),
                IsUpgrade = message.OldFiles.Any(),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                CustomFormatInfo = new WebhookCustomFormatInfo(message.RomInfo.CustomFormats, message.RomInfo.CustomFormatScore)
            };

            if (message.OldFiles.Any())
            {
                payload.DeletedFiles = message.OldFiles.ConvertAll(x => new WebhookRomFile(x.RomFile)
                {
                    Path = Path.Combine(message.Game.Path, x.RomFile.RelativePath),
                    RecycleBinPath = x.RecycleBinPath
                });
            }

            return payload;
        }

        protected WebhookImportCompletePayload BuildOnImportCompletePayload(ImportCompleteMessage message)
        {
            var romFiles = message.RomFiles;

            var payload = new WebhookImportCompletePayload
            {
                EventType = WebhookEventType.Download,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(message.Game),
                Roms = message.Roms.ConvertAll(x => new WebhookEpisode(x)),
                RomFiles = romFiles.ConvertAll(e => new WebhookRomFile(e)),
                Release = new WebhookGrabbedRelease(message.Release, romFiles.First().IndexerFlags, romFiles.First().ReleaseType),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                SourcePath = message.SourcePath,
                DestinationPath = message.DestinationPath
            };

            return payload;
        }

        protected WebhookEpisodeDeletePayload BuildOnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            return new WebhookEpisodeDeletePayload
            {
                EventType = WebhookEventType.RomFileDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(deleteMessage.Game),
                Roms = deleteMessage.RomFile.Roms.Value.ConvertAll(x => new WebhookEpisode(x)),
                RomFile = new WebhookRomFile(deleteMessage.RomFile),
                DeleteReason = deleteMessage.Reason
            };
        }

        protected WebhookSeriesAddPayload BuildOnSeriesAdd(SeriesAddMessage addMessage)
        {
            return new WebhookSeriesAddPayload
            {
                EventType = WebhookEventType.SeriesAdd,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(addMessage.Game),
            };
        }

        protected WebhookSeriesDeletePayload BuildOnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            return new WebhookSeriesDeletePayload
            {
                EventType = WebhookEventType.SeriesDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(deleteMessage.Game),
                DeletedFiles = deleteMessage.DeletedFiles
            };
        }

        protected WebhookRenamePayload BuildOnRenamePayload(Game game, List<RenamedRomFile> renamedFiles)
        {
            return new WebhookRenamePayload
            {
                EventType = WebhookEventType.Rename,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(game),
                RenamedRomFiles = renamedFiles.ConvertAll(x => new WebhookRenamedRomFile(x))
            };
        }

        protected WebhookHealthPayload BuildHealthPayload(HealthCheck.HealthCheck healthCheck)
        {
            return new WebhookHealthPayload
            {
                EventType = WebhookEventType.Health,
                InstanceName = _configFileProvider.InstanceName,
                Level = healthCheck.Type,
                Message = healthCheck.Message,
                Type = healthCheck.Source.Name,
                WikiUrl = healthCheck.WikiUrl?.ToString()
            };
        }

        protected WebhookHealthPayload BuildHealthRestoredPayload(HealthCheck.HealthCheck healthCheck)
        {
            return new WebhookHealthPayload
            {
                EventType = WebhookEventType.HealthRestored,
                InstanceName = _configFileProvider.InstanceName,
                Level = healthCheck.Type,
                Message = healthCheck.Message,
                Type = healthCheck.Source.Name,
                WikiUrl = healthCheck.WikiUrl?.ToString()
            };
        }

        protected WebhookApplicationUpdatePayload BuildApplicationUpdatePayload(ApplicationUpdateMessage updateMessage)
        {
            return new WebhookApplicationUpdatePayload
            {
                EventType = WebhookEventType.ApplicationUpdate,
                InstanceName = _configFileProvider.InstanceName,
                Message = updateMessage.Message,
                PreviousVersion = updateMessage.PreviousVersion.ToString(),
                NewVersion = updateMessage.NewVersion.ToString()
            };
        }

        protected WebhookManualInteractionPayload BuildManualInteractionRequiredPayload(ManualInteractionRequiredMessage message)
        {
            var remoteRom = message.Rom;
            var quality = message.Quality;

            return new WebhookManualInteractionPayload
            {
                EventType = WebhookEventType.ManualInteractionRequired,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = GetGame(message.Game),
                Roms = remoteRom.Roms.ConvertAll(x => new WebhookEpisode(x)),
                DownloadInfo = new WebhookDownloadClientItem(quality, message.TrackedDownload.DownloadItem),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                DownloadStatus = message.TrackedDownload.Status.ToString(),
                DownloadStatusMessages = message.TrackedDownload.StatusMessages.Select(x => new WebhookDownloadStatusMessage(x)).ToList(),
                CustomFormatInfo = new WebhookCustomFormatInfo(remoteRom.CustomFormats, remoteRom.CustomFormatScore),
                Release = new WebhookGrabbedRelease(message.Release)
            };
        }

        protected WebhookPayload BuildTestPayload()
        {
            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Test,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Game = new WebhookSeries
                {
                    Id = 1,
                    Title = "Test Title",
                    Path = "C:\\testpath",
                    IgdbId = 1234,
                    Tags = new List<string> { "test-tag" }
                },
                Roms = new List<WebhookEpisode>
                {
                    new()
                    {
                        Id = 123,
                        EpisodeNumber = 1,
                        PlatformNumber = 1,
                        Title = "Test title"
                    }
                }
            };
        }

        private WebhookSeries GetGame(Game game)
        {
            if (game == null)
            {
                return null;
            }

            _mediaCoverService.ConvertToLocalUrls(game.Id, game.Images);

            return new WebhookSeries(game, GetTagLabels(game));
        }

        private List<string> GetTagLabels(Game game)
        {
            if (game == null)
            {
                return null;
            }

            return _tagRepository.GetTags(game.Tags)
                .Select(s => s.Label)
                .Where(l => l.IsNotNullOrWhiteSpace())
                .OrderBy(l => l)
                .ToList();
        }
    }
}
