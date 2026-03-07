using System;
using System.Collections.Generic;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore.Events;
using Playarr.Core.Download;
using Playarr.Core.Download.Clients;
using Playarr.Core.Localization;
using Playarr.Core.RemotePathMappings;
using Playarr.Core.RootFolders;
using Playarr.Core.ThingiProvider.Events;

namespace Playarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ModelEvent<RootFolder>))]
    [CheckOn(typeof(ModelEvent<RemotePathMapping>))]

    public class DownloadClientSortingCheck : HealthCheckBase, IProvideHealthCheck
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public DownloadClientSortingCheck(IProvideDownloadClient downloadClientProvider,
                                          Logger logger,
                                          ILocalizationService localizationService)
            : base(localizationService)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var clients = _downloadClientProvider.GetDownloadClients(true);

            foreach (var client in clients)
            {
                try
                {
                    var clientName = client.Definition.Name;
                    var status = client.GetStatus();

                    if (status.SortingMode.IsNotNullOrWhiteSpace())
                    {
                        return new HealthCheck(GetType(),
                            HealthCheckResult.Warning,
                            HealthCheckReason.DownloadClientSorting,
                            _localizationService.GetLocalizedString("DownloadClientSortingHealthCheckMessage", new Dictionary<string, object>
                            {
                                { "downloadClientName", clientName },
                                { "sortingMode", status.SortingMode }
                            }),
                            "#download-folder-and-library-folder-not-different-folders");
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occurred in DownloadClientSortingCheck HealthCheck");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
