using System.Net;
using System.Threading.Tasks;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Exceptions;
using Playarr.Core.Indexers;
using Playarr.Core.Localization;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.RemotePathMappings;
using Playarr.Core.ThingiProvider;

namespace Playarr.Core.Download
{
    public abstract class UsenetClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;
        private readonly IValidateNzbs _nzbValidationService;

        protected UsenetClientBase(IHttpClient httpClient,
                                   IConfigService configService,
                                   IDiskProvider diskProvider,
                                   IRemotePathMappingService remotePathMappingService,
                                   IValidateNzbs nzbValidationService,
                                   Logger logger,
                                   ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _httpClient = httpClient;
            _nzbValidationService = nzbValidationService;
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        protected abstract string AddFromNzbFile(RemoteRom remoteRom, string filename, byte[] fileContent);

        public override async Task<string> Download(RemoteRom remoteRom, IIndexer indexer)
        {
            var url = remoteRom.Release.DownloadUrl;
            var filename =  FileNameBuilder.CleanFileName(remoteRom.Release.Title) + ".nzb";

            byte[] nzbData;

            try
            {
                var request = indexer?.GetDownloadRequest(url) ?? new HttpRequest(url);
                request.RateLimitKey = remoteRom?.Release?.IndexerId.ToString();
                request.AllowAutoRedirect = true;

                var response = await RetryStrategy
                    .ExecuteAsync(static async (state, _) => await state._httpClient.GetAsync(state.request), (_httpClient, request))
                    .ConfigureAwait(false);

                nzbData = response.ResponseData;

                _logger.Debug("Downloaded nzb for rom '{0}' finished ({1} bytes from {2})", remoteRom.Release.Title, nzbData.Length, url);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    _logger.Error(ex, "Downloading nzb file for rom '{0}' failed since it no longer exists ({1})", remoteRom.Release.Title, url);
                    throw new ReleaseUnavailableException(remoteRom.Release, "Downloading nzb failed", ex);
                }

                if ((int)ex.Response.StatusCode == 429)
                {
                    _logger.Error("API Grab Limit reached for {0}", url);
                }
                else
                {
                    _logger.Error(ex, "Downloading nzb for rom '{0}' failed ({1})", remoteRom.Release.Title, url);
                }

                throw new ReleaseDownloadException(remoteRom.Release, "Downloading nzb failed", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Downloading nzb for rom '{0}' failed ({1})", remoteRom.Release.Title, url);

                throw new ReleaseDownloadException(remoteRom.Release, "Downloading nzb failed", ex);
            }

            _nzbValidationService.Validate(filename, nzbData);

            _logger.Info("Adding report [{0}] to the queue.", remoteRom.Release.Title);
            return AddFromNzbFile(remoteRom, filename, nzbData);
        }
    }
}
