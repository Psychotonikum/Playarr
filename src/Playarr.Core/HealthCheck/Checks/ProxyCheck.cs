using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using Playarr.Common.Cloud;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Configuration.Events;
using Playarr.Core.Localization;

namespace Playarr.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ProxyCheck : HealthCheckBase
    {
        private readonly Logger _logger;
        private readonly IConfigService _configService;
        private readonly IHttpClient _client;

        private readonly IHttpRequestBuilderFactory _cloudRequestBuilder;

        public ProxyCheck(IPlayarrCloudRequestBuilder cloudRequestBuilder, IConfigService configService, IHttpClient client, Logger logger, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configService = configService;
            _client = client;
            _logger = logger;

            _cloudRequestBuilder = cloudRequestBuilder.Services;
        }

        public override HealthCheck Check()
        {
            if (!_configService.ProxyEnabled)
            {
                return new HealthCheck(GetType());
            }

            var addresses = Dns.GetHostAddresses(_configService.ProxyHostname);

            if (!addresses.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.ProxyResolveIp,
                    _localizationService.GetLocalizedString("ProxyResolveIpHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "proxyHostName", _configService.ProxyHostname }
                    }),
                    "#proxy-failed-resolve-ip");
            }

            var request = _cloudRequestBuilder.Create()
                .Resource("/ping")
                .Build();

            try
            {
                var response = _client.Execute(request);

                // We only care about 400 responses, other error codes can be ignored
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.Error("Proxy Health Check failed: {0}", response.StatusCode);

                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        HealthCheckReason.ProxyBadRequest,
                        _localizationService.GetLocalizedString("ProxyBadRequestHealthCheckMessage", new Dictionary<string, object>
                        {
                            { "statusCode", response.StatusCode }
                        }),
                        "#proxy-failed-test");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Proxy Health Check failed");

                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.ProxyFailed,
                    _localizationService.GetLocalizedString("ProxyFailedToTestHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "url", request.Url }
                    }),
                    "#proxy-failed-test");
            }

            return new HealthCheck(GetType());
        }
    }
}
