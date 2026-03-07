using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentValidation.Results;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Blocklisting;
using Playarr.Core.Configuration;
using Playarr.Core.Download.Clients.Flood.Models;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles.TorrentInfo;
using Playarr.Core.Parser.Model;
using Playarr.Core.RemotePathMappings;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Download.Clients.Flood
{
    public class Flood : TorrentClientBase<FloodSettings>
    {
        private readonly IFloodProxy _proxy;
        private readonly IDownloadSeedConfigProvider _downloadSeedConfigProvider;

        public Flood(IFloodProxy proxy,
                        IDownloadSeedConfigProvider downloadSeedConfigProvider,
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
            _proxy = proxy;
            _downloadSeedConfigProvider = downloadSeedConfigProvider;
        }

        private static IEnumerable<string> HandleTags(RemoteEpisode remoteRom, FloodSettings settings)
        {
            var result = new HashSet<string>();

            if (settings.Tags.Any())
            {
                result.UnionWith(settings.Tags);
            }

            if (settings.AdditionalTags.Any())
            {
                foreach (var additionalTag in settings.AdditionalTags)
                {
                    switch (additionalTag)
                    {
                        case (int)AdditionalTags.TitleSlug:
                            result.Add(remoteRom.Game.TitleSlug);
                            break;
                        case (int)AdditionalTags.Quality:
                            result.Add(remoteRom.ParsedRomInfo.Quality.Quality.ToString());
                            break;
                        case (int)AdditionalTags.Languages:
                            result.UnionWith(remoteRom.Languages.ConvertAll(language => language.ToString()));
                            break;
                        case (int)AdditionalTags.ReleaseGroup:
                            result.Add(remoteRom.ParsedRomInfo.ReleaseGroup);
                            break;
                        case (int)AdditionalTags.Year:
                            result.Add(remoteRom.Game.Year.ToString());
                            break;
                        case (int)AdditionalTags.Indexer:
                            result.Add(remoteRom.Release.Indexer);
                            break;
                        case (int)AdditionalTags.Network:
                            result.Add(remoteRom.Game.Network);
                            break;
                        default:
                            throw new DownloadClientException("Unexpected additional tag ID");
                    }
                }
            }

            return result.Where(t => t.IsNotNullOrWhiteSpace());
        }

        public override string Name => "Flood";
        public override ProviderMessage Message => new ProviderMessage(_localizationService.GetLocalizedString("DownloadClientFloodSettingsRemovalInfo"), ProviderMessageType.Info);

        protected override string AddFromTorrentFile(RemoteEpisode remoteRom, string hash, string filename, byte[] fileContent)
        {
            _proxy.AddTorrentByFile(Convert.ToBase64String(fileContent), HandleTags(remoteRom, Settings), Settings);

            return hash;
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteRom, string hash, string magnetLink)
        {
            _proxy.AddTorrentByUrl(magnetLink, HandleTags(remoteRom, Settings), Settings);

            return hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var items = new List<DownloadClientItem>();

            var list = _proxy.GetTorrents(Settings);

            foreach (var torrent in list)
            {
                var properties = torrent.Value;

                if (!Settings.Tags.All(tag => properties.Tags.Contains(tag)))
                {
                    continue;
                }

                if (Settings.PostImportTags.All(tag => properties.Tags.Contains(tag)))
                {
                    continue;
                }

                var item = new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, Settings.PostImportTags.Any()),
                    DownloadId = torrent.Key,
                    Title = properties.Name,
                    OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(properties.Directory)),
                    Category = properties.Tags.Count > 0 ? properties.Tags[0] : null,
                    RemainingSize = properties.SizeBytes - properties.BytesDone,
                    TotalSize = properties.SizeBytes,
                    SeedRatio = properties.Ratio,
                    Message = properties.Message,
                    CanMoveFiles = false,
                    CanBeRemoved = false,
                };

                if (properties.Eta > 0)
                {
                    item.RemainingTime = TimeSpan.FromSeconds(properties.Eta);
                }

                if (properties.Status.Contains("seeding") || properties.Status.Contains("complete"))
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (properties.Status.Contains("stopped"))
                {
                    item.Status = DownloadItemStatus.Paused;
                }
                else if (properties.Status.Contains("error"))
                {
                    item.Status = DownloadItemStatus.Warning;
                }
                else if (properties.Status.Contains("downloading"))
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                if (item.DownloadClientInfo.RemoveCompletedDownloads && item.Status == DownloadItemStatus.Completed)
                {
                    // Grab cached seedConfig
                    var seedConfig = _downloadSeedConfigProvider.GetSeedConfiguration(item.DownloadId);

                    if (seedConfig != null)
                    {
                        if (item.SeedRatio >= seedConfig.Ratio)
                        {
                            // Check if seed ratio reached
                            item.CanMoveFiles = item.CanBeRemoved = true;
                        }
                        else if (properties.DateFinished is > 0)
                        {
                            // Check if seed time reached
                            if ((DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds((long)properties.DateFinished)) >= seedConfig.SeedTime)
                            {
                                item.CanMoveFiles = item.CanBeRemoved = true;
                            }
                        }
                    }
                }

                items.Add(item);
            }

            return items;
        }

        public override DownloadClientItem GetImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt)
        {
            var result = item.Clone();

            var contentPaths = _proxy.GetTorrentContentPaths(item.DownloadId, Settings);

            if (contentPaths.Count < 1)
            {
                throw new DownloadClientUnavailableException($"Failed to fetch list of contents of torrent: {item.DownloadId}");
            }

            if (contentPaths.Count == 1)
            {
                // For single-file torrent, OutputPath should be the path of file.
                result.OutputPath = item.OutputPath + new OsPath(contentPaths[0]);
            }
            else
            {
                // For multi-file torrent, OutputPath should be the path of base directory of torrent.
                var baseDirectoryPaths = contentPaths.ConvertAll(path =>
                    path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)[0]);

                // Check first segment (directory) of paths of contents. If all contents share the same directory, use that directory.
                if (baseDirectoryPaths.TrueForAll(path => path == baseDirectoryPaths[0]))
                {
                    result.OutputPath = item.OutputPath + new OsPath(baseDirectoryPaths[0]);
                }

                // Otherwise, OutputPath is already the base directory.
            }

            return result;
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            if (Settings.PostImportTags.Any())
            {
                var list = _proxy.GetTorrents(Settings);

                if (list.ContainsKey(downloadClientItem.DownloadId))
                {
                    _proxy.SetTorrentsTags(downloadClientItem.DownloadId,
                        list[downloadClientItem.DownloadId].Tags.Concat(Settings.PostImportTags).ToImmutableHashSet(),
                        Settings);
                }
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            _proxy.DeleteTorrent(item.DownloadId, deleteData, Settings);
        }

        public override DownloadClientInfo GetStatus()
        {
            var destDir = _proxy.GetClientSettings(Settings).DirectoryDefault;

            if (Settings.Destination.IsNotNullOrWhiteSpace())
            {
                destDir = Settings.Destination;
            }

            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "::1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(destDir)) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            try
            {
                _proxy.AuthVerify(Settings);
            }
            catch (DownloadClientAuthenticationException ex)
            {
                failures.Add(new ValidationFailure("Password", ex.Message));
            }
            catch (Exception ex)
            {
                failures.Add(new ValidationFailure("Host", ex.Message));
            }
        }
    }
}
