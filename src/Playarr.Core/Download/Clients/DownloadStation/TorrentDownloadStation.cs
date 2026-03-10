using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Blocklisting;
using Playarr.Core.Configuration;
using Playarr.Core.Download.Clients.DownloadStation.Proxies;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles.TorrentInfo;
using Playarr.Core.Parser.Model;
using Playarr.Core.RemotePathMappings;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.Download.Clients.DownloadStation
{
    public class TorrentDownloadStation : TorrentClientBase<DownloadStationSettings>
    {
        protected readonly IDownloadStationInfoProxy _dsInfoProxy;
        protected readonly IDownloadStationTaskProxySelector _dsTaskProxySelector;
        protected readonly ISharedFolderResolver _sharedFolderResolver;
        protected readonly ISerialNumberProvider _serialNumberProvider;
        protected readonly IFileStationProxy _fileStationProxy;

        public TorrentDownloadStation(ISharedFolderResolver sharedFolderResolver,
                                      ISerialNumberProvider serialNumberProvider,
                                      IFileStationProxy fileStationProxy,
                                      IDownloadStationInfoProxy dsInfoProxy,
                                      IDownloadStationTaskProxySelector dsTaskProxySelector,
                                      ITorrentFileInfoReader torrentFileInfoReader,
                                      IHttpClient httpClient,
                                      IConfigService configService,
                                      IDiskProvider diskProvider,
                                      IRemotePathMappingService remotePathMappingService,
                                      ILocalizationService localizationService,
                                      IBlocklistService blocklistService,
                                      Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
            _dsInfoProxy = dsInfoProxy;
            _dsTaskProxySelector = dsTaskProxySelector;
            _fileStationProxy = fileStationProxy;
            _sharedFolderResolver = sharedFolderResolver;
            _serialNumberProvider = serialNumberProvider;
        }

        public override string Name => "Download Station";

        public override ProviderMessage Message => new ProviderMessage(_localizationService.GetLocalizedString("DownloadClientDownloadStationProviderMessage"), ProviderMessageType.Warning);

        private IDownloadStationTaskProxy DsTaskProxy => _dsTaskProxySelector.GetProxy(Settings);

        protected IEnumerable<DownloadStationTask> GetTasks()
        {
            return DsTaskProxy.GetTasks(Settings).Where(v => v.Type.ToLower() == DownloadStationTaskType.BT.ToString().ToLower());
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var torrents = GetTasks();
            var serialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                var outputPath = new OsPath($"/{torrent.Additional.Detail["destination"]}");

                if (Settings.GameDirectory.IsNotNullOrWhiteSpace())
                {
                    if (!new OsPath($"/{Settings.GameDirectory}").Contains(outputPath))
                    {
                        continue;
                    }
                }
                else if (Settings.GameCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(Settings.GameCategory))
                    {
                        continue;
                    }
                }

                var item = new DownloadClientItem
                {
                    Category = Settings.GameCategory,
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = CreateDownloadId(torrent.Id, serialNumber),
                    Title = torrent.Title,
                    TotalSize = torrent.Size,
                    RemainingSize = GetRemainingSize(torrent),
                    RemainingTime = GetRemainingTime(torrent),
                    SeedRatio = GetSeedRatio(torrent),
                    Status = GetStatus(torrent),
                    Message = GetMessage(torrent)
                };

                item.CanMoveFiles = item.CanBeRemoved = item.DownloadClientInfo.RemoveCompletedDownloads && IsFinished(torrent);

                if (item.Status == DownloadItemStatus.Completed || item.Status == DownloadItemStatus.Failed)
                {
                    item.OutputPath = GetOutputPath(outputPath, torrent, serialNumber);
                }

                items.Add(item);
            }

            return items;
        }

        public override DownloadClientInfo GetStatus()
        {
            try
            {
                var serialNumber = _serialNumberProvider.GetSerialNumber(Settings);

                // Download station returns the path without the leading `/`, but the leading
                // slash is required to get the full path back from download station.
                var path = new OsPath($"/{GetDownloadDirectory()}");

                var fullPath = _sharedFolderResolver.RemapToFullPath(path, Settings, serialNumber);

                return new DownloadClientInfo
                {
                    IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                    OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, fullPath) }
                };
            }
            catch (DownloadClientException e)
            {
                _logger.Debug(e, "Failed to get config from Download Station");

                throw;
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(item);
            }

            DsTaskProxy.RemoveTask(ParseDownloadId(item.DownloadId), Settings);
            _logger.Debug("{0} removed correctly", item.DownloadId);
        }

        protected OsPath GetOutputPath(OsPath outputPath, DownloadStationTask torrent, string serialNumber)
        {
            var fullPath = _sharedFolderResolver.RemapToFullPath(outputPath, Settings, serialNumber);

            var remotePath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, fullPath);

            var finalPath = remotePath + torrent.Title;

            return finalPath;
        }

        protected override string AddFromMagnetLink(RemoteRom remoteRom, string hash, string magnetLink)
        {
            var hashedSerialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            DsTaskProxy.AddTaskFromUrl(magnetLink, GetDownloadDirectory(), Settings);

            var item = GetTasks().SingleOrDefault(t => t.Additional.Detail["uri"] == magnetLink);

            if (item != null)
            {
                _logger.Debug("{0} added correctly", remoteRom);
                return CreateDownloadId(item.Id, hashedSerialNumber);
            }

            _logger.Debug("No such task {0} in Download Station", magnetLink);

            throw new DownloadClientException("Failed to add magnet task to Download Station");
        }

        protected override string AddFromTorrentFile(RemoteRom remoteRom, string hash, string filename, byte[] fileContent)
        {
            var hashedSerialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            DsTaskProxy.AddTaskFromData(fileContent, filename, GetDownloadDirectory(), Settings);

            var items = GetTasks().Where(t => t.Additional.Detail["uri"] == Path.GetFileNameWithoutExtension(filename));

            var item = items.SingleOrDefault();

            if (item != null)
            {
                _logger.Debug("{0} added correctly", remoteRom);
                return CreateDownloadId(item.Id, hashedSerialNumber);
            }

            _logger.Debug("No such task {0} in Download Station", filename);

            throw new DownloadClientException("Failed to add torrent task to Download Station");
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestOutputPath());
            failures.AddIfNotNull(TestGetTorrents());
        }

        protected bool IsFinished(DownloadStationTask torrent)
        {
            return torrent.Status == DownloadStationTaskStatus.Finished;
        }

        protected string GetMessage(DownloadStationTask torrent)
        {
            if (torrent.StatusExtra != null)
            {
                if (torrent.Status == DownloadStationTaskStatus.Extracting)
                {
                    return _localizationService.GetLocalizedString("DownloadStationStatusExtracting",
                        new Dictionary<string, object>
                            { { "progress", int.Parse(torrent.StatusExtra["unzip_progress"]) } });
                }

                if (torrent.Status == DownloadStationTaskStatus.Error)
                {
                    return torrent.StatusExtra["error_detail"];
                }
            }

            return null;
        }

        protected DownloadItemStatus GetStatus(DownloadStationTask torrent)
        {
            switch (torrent.Status)
            {
                case DownloadStationTaskStatus.Unknown:
                case DownloadStationTaskStatus.Waiting:
                case DownloadStationTaskStatus.FilehostingWaiting:
                    return torrent.Size == 0 || GetRemainingSize(torrent) > 0 ? DownloadItemStatus.Queued : DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Paused:
                    return DownloadItemStatus.Paused;
                case DownloadStationTaskStatus.Finished:
                case DownloadStationTaskStatus.Seeding:
                    return DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Error:
                    return DownloadItemStatus.Failed;
            }

            return DownloadItemStatus.Downloading;
        }

        protected long GetRemainingSize(DownloadStationTask torrent)
        {
            var downloadedString = torrent.Additional.Transfer["size_downloaded"];

            if (downloadedString.IsNullOrWhiteSpace() || !long.TryParse(downloadedString, out var downloadedSize))
            {
                _logger.Debug("Torrent {0} has invalid size_downloaded: {1}", torrent.Title, downloadedString);
                downloadedSize = 0;
            }

            return torrent.Size - Math.Max(0, downloadedSize);
        }

        protected TimeSpan? GetRemainingTime(DownloadStationTask torrent)
        {
            var speedString = torrent.Additional.Transfer["speed_download"];

            if (speedString.IsNullOrWhiteSpace() || !long.TryParse(speedString, out var downloadSpeed))
            {
                _logger.Debug("Torrent {0} has invalid speed_download: {1}", torrent.Title, speedString);
                downloadSpeed = 0;
            }

            if (downloadSpeed <= 0)
            {
                return null;
            }

            var remainingSize = GetRemainingSize(torrent);

            return TimeSpan.FromSeconds(remainingSize / downloadSpeed);
        }

        protected double? GetSeedRatio(DownloadStationTask torrent)
        {
            var downloaded = torrent.Additional.Transfer["size_downloaded"].ParseInt64();
            var uploaded = torrent.Additional.Transfer["size_uploaded"].ParseInt64();

            if (downloaded.HasValue && uploaded.HasValue)
            {
                return downloaded <= 0 ? 0 : (double)uploaded.Value / downloaded.Value;
            }

            return null;
        }

        protected ValidationFailure TestOutputPath()
        {
            try
            {
                var downloadDir = GetDownloadDirectory();

                if (downloadDir == null)
                {
                    return new PlayarrValidationFailure(nameof(Settings.GameDirectory), _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationNoDefaultDestination"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationNoDefaultDestinationDetail", new Dictionary<string, object> { { "username", Settings.Username } })
                    };
                }

                var sharedFolder = downloadDir.Split('\\', '/')[0];
                var fieldName = Settings.GameDirectory.IsNotNullOrWhiteSpace() ? nameof(Settings.GameDirectory) : nameof(Settings.GameCategory);

                var folderInfo = _fileStationProxy.GetInfoFileOrDirectory($"/{downloadDir}", Settings);

                if (folderInfo.Additional == null)
                {
                    return new PlayarrValidationFailure(fieldName, _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationSharedFolderMissing"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationSharedFolderMissingDetail", new Dictionary<string, object> { { "sharedFolder", sharedFolder } })
                    };
                }

                if (!folderInfo.IsDir)
                {
                    return new PlayarrValidationFailure(fieldName, _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationFolderMissing"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationFolderMissingDetail", new Dictionary<string, object> { { "downloadDir", downloadDir }, { "sharedFolder", sharedFolder } })
                    };
                }

                return null;
            }
            catch (DownloadClientAuthenticationException ex)
            {
                // User could not have permission to access to downloadstation
                _logger.Error(ex, ex.Message);
                return new PlayarrValidationFailure(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error testing Torrent Download Station");
                return new PlayarrValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
            }
        }

        protected ValidationFailure TestConnection()
        {
            try
            {
                return ValidateVersion();
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);
                return new PlayarrValidationFailure("Username", _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailure"))
                {
                    DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailureDetail", new Dictionary<string, object> { { "clientName", Name } })
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Unable to connect to Torrent Download Station");

                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return new PlayarrValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnectDetail")
                    };
                }

                return new PlayarrValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error testing Torrent Download Station");

                return new PlayarrValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                       {
                           DetailedDescription = ex.Message
                       };
            }
        }

        protected ValidationFailure ValidateVersion()
        {
            var info = DsTaskProxy.GetApiInfo(Settings);

            _logger.Debug("Download Station api version information: Min {0} - Max {1}", info.MinVersion, info.MaxVersion);

            if (info.MinVersion > 2 || info.MaxVersion < 2)
            {
                return new ValidationFailure(string.Empty,
                    _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationApiVersion",
                        new Dictionary<string, object>
                        {
                            { "requiredVersion", 2 }, { "minVersion", info.MinVersion }, { "maxVersion", info.MaxVersion }
                        }));
            }

            return null;
        }

        protected ValidationFailure TestGetTorrents()
        {
            try
            {
                GetItems();
                return null;
            }
            catch (Exception ex)
            {
                return new PlayarrValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationTestTorrents", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
        }

        protected string ParseDownloadId(string id)
        {
            return id.Split(':')[1];
        }

        protected string CreateDownloadId(string id, string hashedSerialNumber)
        {
            return $"{hashedSerialNumber}:{id}";
        }

        protected string GetDefaultDir()
        {
            var config = _dsInfoProxy.GetConfig(Settings);

            var path = config["default_destination"] as string;

            return path;
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.GameDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.GameDirectory.TrimStart('/');
            }

            var destDir = GetDefaultDir();

            if (destDir.IsNotNullOrWhiteSpace() && Settings.GameCategory.IsNotNullOrWhiteSpace())
            {
                return $"{destDir.TrimEnd('/')}/{Settings.GameCategory}";
            }

            return destDir.TrimEnd('/');
        }
    }
}
