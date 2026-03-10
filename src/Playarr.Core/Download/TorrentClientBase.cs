using System;
using System.Net;
using System.Threading.Tasks;
using MonoTorrent;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Blocklisting;
using Playarr.Core.Configuration;
using Playarr.Core.Exceptions;
using Playarr.Core.Indexers;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles.TorrentInfo;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.RemotePathMappings;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Download
{
    public abstract class TorrentClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;
        private readonly IBlocklistService _blocklistService;
        protected readonly ITorrentFileInfoReader _torrentFileInfoReader;

        protected TorrentClientBase(ITorrentFileInfoReader torrentFileInfoReader,
            IHttpClient httpClient,
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            ILocalizationService localizationService,
            IBlocklistService blocklistService,
            Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _httpClient = httpClient;
            _blocklistService = blocklistService;
            _torrentFileInfoReader = torrentFileInfoReader;
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;

        public virtual bool PreferTorrentFile => false;

        protected abstract string AddFromMagnetLink(RemoteRom remoteRom, string hash, string magnetLink);
        protected abstract string AddFromTorrentFile(RemoteRom remoteRom, string hash, string filename, byte[] fileContent);

        public override async Task<string> Download(RemoteRom remoteRom, IIndexer indexer)
        {
            var torrentInfo = remoteRom.Release as TorrentInfo;

            string magnetUrl = null;
            string torrentUrl = null;

            if (remoteRom.Release.DownloadUrl.IsNotNullOrWhiteSpace() && remoteRom.Release.DownloadUrl.StartsWith("magnet:"))
            {
                magnetUrl = remoteRom.Release.DownloadUrl;
            }
            else
            {
                torrentUrl = remoteRom.Release.DownloadUrl;
            }

            if (torrentInfo != null && !torrentInfo.MagnetUrl.IsNullOrWhiteSpace())
            {
                magnetUrl = torrentInfo.MagnetUrl;
            }

            if (PreferTorrentFile)
            {
                if (torrentUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return await DownloadFromWebUrl(remoteRom, indexer, torrentUrl);
                    }
                    catch (Exception ex)
                    {
                        if (!magnetUrl.IsNullOrWhiteSpace())
                        {
                            throw;
                        }

                        _logger.Debug("Torrent download failed, trying magnet. ({0})", ex.Message);
                    }
                }

                if (magnetUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromMagnetUrl(remoteRom, indexer, magnetUrl);
                    }
                    catch (NotSupportedException ex)
                    {
                        throw new ReleaseDownloadException(remoteRom.Release, "Magnet not supported by download client. ({0})", ex.Message);
                    }
                }
            }
            else
            {
                if (magnetUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromMagnetUrl(remoteRom, indexer, magnetUrl);
                    }
                    catch (NotSupportedException ex)
                    {
                        if (torrentUrl.IsNullOrWhiteSpace())
                        {
                            throw new ReleaseDownloadException(remoteRom.Release, "Magnet not supported by download client. ({0})", ex.Message);
                        }

                        _logger.Debug("Magnet not supported by download client, trying torrent. ({0})", ex.Message);
                    }
                }

                if (torrentUrl.IsNotNullOrWhiteSpace())
                {
                    return await DownloadFromWebUrl(remoteRom, indexer, torrentUrl);
                }
            }

            return null;
        }

        private async Task<string> DownloadFromWebUrl(RemoteRom remoteRom, IIndexer indexer, string torrentUrl)
        {
            byte[] torrentFile = null;

            try
            {
                var request = indexer?.GetDownloadRequest(torrentUrl) ?? new HttpRequest(torrentUrl);
                request.RateLimitKey = remoteRom?.Release?.IndexerId.ToString();
                request.Headers.Accept = "application/x-bittorrent";
                request.AllowAutoRedirect = false;

                var response = await RetryStrategy
                    .ExecuteAsync(static async (state, _) => await state._httpClient.GetAsync(state.request), (_httpClient, request))
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.SeeOther)
                {
                    var locationHeader = response.Headers.GetSingleValue("Location");

                    _logger.Trace("Torrent request is being redirected to: {0}", locationHeader);

                    if (locationHeader != null)
                    {
                        if (locationHeader.StartsWith("magnet:"))
                        {
                            return DownloadFromMagnetUrl(remoteRom, indexer, locationHeader);
                        }

                        request.Url += new HttpUri(locationHeader);

                        return await DownloadFromWebUrl(remoteRom, indexer, request.Url.ToString());
                    }

                    throw new WebException("Remote website tried to redirect without providing a location.");
                }

                torrentFile = response.ResponseData;

                _logger.Debug("Downloading torrent for rom '{0}' finished ({1} bytes from {2})", remoteRom.Release.Title, torrentFile.Length, torrentUrl);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    _logger.Error(ex, "Downloading torrent file for rom '{0}' failed since it no longer exists ({1})", remoteRom.Release.Title, torrentUrl);
                    throw new ReleaseUnavailableException(remoteRom.Release, "Downloading torrent failed", ex);
                }

                if ((int)ex.Response.StatusCode == 429)
                {
                    _logger.Error("API Grab Limit reached for {0}", torrentUrl);
                }
                else
                {
                    _logger.Error(ex, "Downloading torrent file for rom '{0}' failed ({1})", remoteRom.Release.Title, torrentUrl);
                }

                throw new ReleaseDownloadException(remoteRom.Release, "Downloading torrent failed", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Downloading torrent file for rom '{0}' failed ({1})", remoteRom.Release.Title, torrentUrl);

                throw new ReleaseDownloadException(remoteRom.Release, "Downloading torrent failed", ex);
            }

            var filename = string.Format("{0}.torrent", FileNameBuilder.CleanFileName(remoteRom.Release.Title));
            var hash = _torrentFileInfoReader.GetHashFromTorrentFile(torrentFile);

            EnsureReleaseIsNotBlocklisted(remoteRom, indexer, hash);

            var actualHash = AddFromTorrentFile(remoteRom, hash, filename, torrentFile);

            if (actualHash.IsNotNullOrWhiteSpace() && hash != actualHash)
            {
                _logger.Debug(
                    "{0} did not return the expected InfoHash for '{1}', Playarr could potentially lose track of the download in progress.",
                    Definition.Implementation,
                    remoteRom.Release.DownloadUrl);
            }

            return actualHash;
        }

        private string DownloadFromMagnetUrl(RemoteRom remoteRom, IIndexer indexer, string magnetUrl)
        {
            string hash = null;
            string actualHash = null;

            try
            {
                hash = MagnetLink.Parse(magnetUrl).InfoHashes.V1OrV2.ToHex();
            }
            catch (FormatException ex)
            {
                throw new ReleaseDownloadException(remoteRom.Release, "Failed to parse magnetlink for rom '{0}': '{1}'", ex, remoteRom.Release.Title, magnetUrl);
            }

            if (hash != null)
            {
                EnsureReleaseIsNotBlocklisted(remoteRom, indexer, hash);

                actualHash = AddFromMagnetLink(remoteRom, hash, magnetUrl);
            }

            if (actualHash.IsNotNullOrWhiteSpace() && hash != actualHash)
            {
                _logger.Debug(
                    "{0} did not return the expected InfoHash for '{1}', Playarr could potentially lose track of the download in progress.",
                    Definition.Implementation,
                    remoteRom.Release.DownloadUrl);
            }

            return actualHash;
        }

        private void EnsureReleaseIsNotBlocklisted(RemoteRom remoteRom, IIndexer indexer, string hash)
        {
            var indexerSettings = indexer?.Definition?.Settings as ITorrentIndexerSettings;
            var torrentInfo = remoteRom.Release as TorrentInfo;
            var torrentInfoHash = torrentInfo?.InfoHash;

            // If the release didn't come from an interactive search,
            // the hash wasn't known during processing and the
            // indexer is configured to reject blocklisted releases
            // during grab check if it's already been blocklisted.

            if (torrentInfo != null && torrentInfoHash.IsNullOrWhiteSpace())
            {
                // If the hash isn't known from parsing we set it here so it can be used for blocklisting.
                torrentInfo.InfoHash = hash;

                if (remoteRom.ReleaseSource != ReleaseSourceType.InteractiveSearch &&
                    indexerSettings?.RejectBlocklistedTorrentHashesWhileGrabbing == true &&
                    _blocklistService.BlocklistedTorrentHash(remoteRom.Game.Id, hash))
                {
                    throw new ReleaseBlockedException(remoteRom.Release, "Release previously added to blocklist");
                }
            }
        }
    }
}
