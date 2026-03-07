using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Playarr.Common.EnsureThat;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Common.TPL;
using Playarr.Core.Download.Clients;
using Playarr.Core.Download.Pending;
using Playarr.Core.Exceptions;
using Playarr.Core.Indexers;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download
{
    public interface IDownloadService
    {
        Task DownloadReport(RemoteEpisode remoteRom, int? downloadClientId);
    }

    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly IIndexerFactory _indexerFactory;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISeedConfigProvider _seedConfigProvider;
        private readonly Logger _logger;

        public DownloadService(IProvideDownloadClient downloadClientProvider,
                               IDownloadClientStatusService downloadClientStatusService,
                               IIndexerFactory indexerFactory,
                               IIndexerStatusService indexerStatusService,
                               IRateLimitService rateLimitService,
                               IEventAggregator eventAggregator,
                               ISeedConfigProvider seedConfigProvider,
                               Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _downloadClientStatusService = downloadClientStatusService;
            _indexerFactory = indexerFactory;
            _indexerStatusService = indexerStatusService;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _seedConfigProvider = seedConfigProvider;
            _logger = logger;
        }

        public async Task DownloadReport(RemoteEpisode remoteRom, int? downloadClientId)
        {
            var filterBlockedClients = remoteRom.Release.PendingReleaseReason == PendingReleaseReason.DownloadClientUnavailable;

            var tags = remoteRom.Game?.Tags;

            if (downloadClientId.HasValue)
            {
                var specificClient = _downloadClientProvider.Get(downloadClientId.Value);
                await DownloadReport(remoteRom, specificClient);

                return;
            }

            var availableClients = _downloadClientProvider.GetDownloadClients(
                remoteRom.Release.DownloadProtocol,
                remoteRom.Release.IndexerId,
                filterBlockedClients,
                tags).ToList();

            if (!availableClients.Any())
            {
                throw new DownloadClientUnavailableException($"No {remoteRom.Release.DownloadProtocol} download client available");
            }

            var triedClients = new HashSet<int>();

            foreach (var downloadClient in availableClients)
            {
                if (triedClients.Contains(downloadClient.Definition.Id))
                {
                    continue;
                }

                try
                {
                    _logger.Debug("Attempting download with client: {0}", downloadClient.Definition.Name);
                    await DownloadReport(remoteRom, downloadClient);

                    _downloadClientProvider.ReportSuccessfulDownloadClient(
                        remoteRom.Release.DownloadProtocol,
                        downloadClient.Definition.Id);

                    return;
                }
                catch (DownloadClientException ex)
                {
                    _logger.Trace(ex, "Unable to add report to download client: {0}", downloadClient.Definition.Name);
                    triedClients.Add(downloadClient.Definition.Id);
                }
                catch (Exception ex)
                {
                    // Rethrow specific exceptions that should not trigger a fallback
                    if (ex is ReleaseDownloadException)
                    {
                        throw;
                    }

                    _logger.Trace(ex, "Unable to add report to download client: {0}", downloadClient.Definition.Name);
                    triedClients.Add(downloadClient.Definition.Id);
                }
            }

            throw new DownloadClientUnavailableException("All '{0}' download clients failed", remoteRom.Release.DownloadProtocol);
        }

        private async Task DownloadReport(RemoteEpisode remoteRom, IDownloadClient downloadClient)
        {
            Ensure.That(remoteRom.Game, () => remoteRom.Game).IsNotNull();
            Ensure.That(remoteRom.Roms, () => remoteRom.Roms).HasItems();

            var downloadTitle = remoteRom.Release.Title;

            if (downloadClient == null)
            {
                throw new DownloadClientUnavailableException($"{remoteRom.Release.DownloadProtocol} Download client isn't configured yet");
            }

            // Get the seed configuration for this release.
            remoteRom.SeedConfiguration = _seedConfigProvider.GetSeedConfiguration(remoteRom);

            // Limit grabs to 2 per second.
            if (remoteRom.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteRom.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteRom.Release.DownloadUrl);
                await _rateLimitService.WaitAndPulseAsync(url.Host, TimeSpan.FromSeconds(2));
            }

            IIndexer indexer = null;

            if (remoteRom.Release.IndexerId > 0)
            {
                indexer = _indexerFactory.GetInstance(_indexerFactory.Get(remoteRom.Release.IndexerId));
            }

            string downloadClientId;
            try
            {
                downloadClientId = await downloadClient.Download(remoteRom, indexer);
                _downloadClientStatusService.RecordSuccess(downloadClient.Definition.Id);
                _indexerStatusService.RecordSuccess(remoteRom.Release.IndexerId);
            }
            catch (ReleaseUnavailableException)
            {
                _logger.Trace("Release {0} no longer available on indexer.", remoteRom);
                throw;
            }
            catch (ReleaseBlockedException)
            {
                _logger.Trace("Release {0} previously added to blocklist, not sending to download client again.", remoteRom);
                throw;
            }
            catch (DownloadClientRejectedReleaseException)
            {
                _logger.Trace("Release {0} rejected by download client, possible duplicate.", remoteRom);
                throw;
            }
            catch (ReleaseDownloadException ex)
            {
                if (ex.InnerException is TooManyRequestsException http429)
                {
                    _indexerStatusService.RecordFailure(remoteRom.Release.IndexerId, http429.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(remoteRom.Release.IndexerId);
                }

                throw;
            }

            var episodeGrabbedEvent = new EpisodeGrabbedEvent(remoteRom);
            episodeGrabbedEvent.DownloadClient = downloadClient.Name;
            episodeGrabbedEvent.DownloadClientId = downloadClient.Definition.Id;
            episodeGrabbedEvent.DownloadClientName = downloadClient.Definition.Name;

            if (downloadClientId.IsNotNullOrWhiteSpace())
            {
                episodeGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to {0}. Indexer {1}. {2}", downloadClient.Definition.Name, remoteRom.Release.Indexer, downloadTitle);
            _eventAggregator.PublishEvent(episodeGrabbedEvent);
        }
    }
}
