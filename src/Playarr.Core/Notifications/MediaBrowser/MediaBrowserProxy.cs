using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Http;
using Playarr.Common.Serializer;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Emby
{
    public class MediaBrowserProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MediaBrowserProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void TestConnection(MediaBrowserSettings settings)
        {
            var path = "/System/Configuration";
            var request = BuildRequest(path, settings);
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Get(request);
            _logger.Trace("Response: {0}", response.Content);
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            var path = "/Notifications/Admin";
            var request = BuildRequest(path, settings);
            request.Headers.ContentType = "application/json";
            request.LogHttpError = false;

            request.SetContent(new
                           {
                               Name = title,
                               Description = message,
                               ImageUrl = "https://raw.github.com/Playarr/Playarr/develop/Logo/64.png"
                           }.ToJson());

            ProcessRequest(request, settings);
        }

        public HashSet<string> GetPaths(MediaBrowserSettings settings, Game game)
        {
            var path = "/Items";
            var url = GetUrl(settings);

            // NameStartsWith uses the sort title, which is not the game title
            var request = new HttpRequestBuilder(url)
                .Resource(path)
                .AddQueryParam("recursive", "true")
                .AddQueryParam("includeItemTypes", "Game")
                .AddQueryParam("fields", "Path,ProviderIds")
                .AddQueryParam("years", game.Year)
                .Build();

            try
            {
                var paths = ProcessGetRequest<MediaBrowserItems>(request, settings).Items.GroupBy(item =>
                {
                    if (item is { ProviderIds.Igdb: int igdbid } && igdbid != 0 && igdbid == game.IgdbId)
                    {
                        return MediaBrowserMatchQuality.Id;
                    }

                    if (item is { ProviderIds.Imdb: string imdbid } && imdbid == game.ImdbId)
                    {
                        return MediaBrowserMatchQuality.Id;
                    }

                    if (item is { ProviderIds.TvMaze: int tvmazeid } && tvmazeid != 0 && tvmazeid == game.RawgId)
                    {
                        return MediaBrowserMatchQuality.Id;
                    }

                    if (item is { ProviderIds.Tmdb: int tmdbid } && tmdbid != 0 && tmdbid == game.TmdbId)
                    {
                        return MediaBrowserMatchQuality.Id;
                    }

                    if (item is { ProviderIds.TvRage: int tvrageid } && tvrageid != 0 && tvrageid == game.MobyGamesId)
                    {
                        return MediaBrowserMatchQuality.Id;
                    }

                    if (item is { Name: var name } && name == game.Title)
                    {
                        return MediaBrowserMatchQuality.Name;
                    }

                    return MediaBrowserMatchQuality.None;
                },
                    item => item.Path).OrderBy(group => (int)group.Key).First();

                if (paths.Key == MediaBrowserMatchQuality.None)
                {
                    _logger.Trace("Could not find game by name");

                    return new HashSet<string>();
                }

                _logger.Trace("Found game by name/id: {0}", string.Join(" ", paths));

                return paths.ToHashSet();
            }
            catch (InvalidOperationException)
            {
                _logger.Trace("Could not find game by name.");

                return new HashSet<string>();
            }
        }

        public void Update(MediaBrowserSettings settings, string seriesPath, string updateType)
        {
            var path = "/Library/Media/Updated";
            var request = BuildRequest(path, settings);
            request.Headers.ContentType = "application/json";

            request.SetContent(new
            {
                Updates = new[]
                {
                    new
                    {
                        Path = seriesPath,
                        UpdateType = updateType
                    }
                }
            }.ToJson());

            ProcessRequest(request, settings);
        }

        private T ProcessGetRequest<T>(HttpRequest request, MediaBrowserSettings settings)
            where T : new()
        {
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Get<T>(request);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Resource;
        }

        private string ProcessRequest(HttpRequest request, MediaBrowserSettings settings)
        {
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Post(request);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private string GetUrl(MediaBrowserSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            return $@"{scheme}://{settings.Address}";
        }

        private HttpRequest BuildRequest(string path, MediaBrowserSettings settings)
        {
            var url = GetUrl(settings);

            return new HttpRequestBuilder(url).Resource(path).Build();
        }

        private void CheckForError(HttpResponse response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            // TODO: actually check for the error
        }
    }
}
