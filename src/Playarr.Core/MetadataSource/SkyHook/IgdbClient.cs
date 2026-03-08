using System;
using System.Threading.Tasks;
using IGDB;
using IGDB.Models;
using NLog;
using Playarr.Core.Configuration;

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
        private readonly Logger _logger;

        private IGDBClient _client;
        private string _configuredClientId;
        private string _configuredClientSecret;

        public IgdbClient(IConfigService configService, Logger logger)
        {
            _configService = configService;
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
            var clientId = _configService.TwitchClientId;
            var clientSecret = _configService.TwitchClientSecret;

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
