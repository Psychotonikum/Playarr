using System;
using System.Linq;
using System.Threading.Tasks;
using IGDB;
using IGDB.Models;
using NLog;
using Playarr.Core.Configuration;
using Playarr.Core.MetadataSource.Providers;

namespace Playarr.Core.MetadataSource.SkyHook
{
    public interface IIgdbClient
    {
        Game[] SearchGames(string query);

        ReleaseDate[] SearchReleaseDates(string query);
    }

    public class IgdbClient : IIgdbClient
    {
        private readonly IConfigService _configService;
        private readonly IMetadataSourceProviderFactory _metadataSourceProviderFactory;
        private readonly Logger _logger;

        private IGDBClient _client;
        private string _configuredClientId;
        private string _configuredClientSecret;

        public IgdbClient(IConfigService configService, IMetadataSourceProviderFactory metadataSourceProviderFactory, Logger logger)
        {
            _configService = configService;
            _metadataSourceProviderFactory = metadataSourceProviderFactory;
            _logger = logger;
        }

        public Game[] SearchGames(string query)
        {
            var client = GetClient();
            return RunSync(client.QueryAsync<Game>(IGDBClient.Endpoints.Games, query));
        }

        public ReleaseDate[] SearchReleaseDates(string query)
        {
            var client = GetClient();
            return RunSync(client.QueryAsync<ReleaseDate>(IGDBClient.Endpoints.ReleaseDates, query));
        }

        private IGDBClient GetClient()
        {
            var clientId = string.Empty;
            var clientSecret = string.Empty;

            // Try V5 provider settings first (IGDB provider configured via UI)
            var igdbDefinition = _metadataSourceProviderFactory.All()
                .FirstOrDefault(d => d.Implementation == "IgdbProvider");

            if (igdbDefinition?.Settings is IgdbProviderSettings igdbSettings &&
                !string.IsNullOrWhiteSpace(igdbSettings.TwitchClientId) &&
                !string.IsNullOrWhiteSpace(igdbSettings.TwitchClientSecret))
            {
                clientId = igdbSettings.TwitchClientId;
                clientSecret = igdbSettings.TwitchClientSecret;
            }
            else
            {
                // Fall back to V3 config
                clientId = _configService.TwitchClientId;
                clientSecret = _configService.TwitchClientSecret;
            }

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException("Twitch Client ID and Secret must be configured in Settings > Metadata Source.");
            }

            if (_client == null || _configuredClientId != clientId || _configuredClientSecret != clientSecret)
            {
                _logger.Debug("Creating IGDB client instance with configured Twitch credentials");
                _client = IGDBClient.CreateWithDefaults(clientId, clientSecret);
                _configuredClientId = clientId;
                _configuredClientSecret = clientSecret;
            }

            return _client;
        }

        private static T RunSync<T>(Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
