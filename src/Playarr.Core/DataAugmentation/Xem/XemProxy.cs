using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using Playarr.Common.Http;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.DataAugmentation.Xem.Model;

namespace Playarr.Core.DataAugmentation.Xem
{
    public interface IXemProxy
    {
        List<int> GetXemGameIds();
        List<XemSceneIgdbMapping> GetSceneIgdbMappings(int id);
        List<SceneMapping> GetSceneIgdbNames();
    }

    public class XemProxy : IXemProxy
    {
        private const string ROOT_URL = "https://thexem.info/map/";

        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _xemRequestBuilder;

        private static readonly string[] IgnoredErrors = { "no single connection", "no show with the igdb_id" };

        public XemProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _xemRequestBuilder = new HttpRequestBuilder(ROOT_URL)
                .AddSuffixQueryParam("origin", "igdb")
                .CreateFactory();
        }

        public List<int> GetXemGameIds()
        {
            _logger.Debug("Fetching Game IDs from");

            var request = _xemRequestBuilder.Create()
                                            .Resource("/havemap")
                                            .Build();

            var response = _httpClient.Get<XemResult<List<string>>>(request).Resource;
            CheckForFailureResult(response);

            return response.Data.Select(d =>
            {
                int.TryParse(d, out var igdbId);

                return igdbId;
            }).Where(t => t > 0).ToList();
        }

        public List<XemSceneIgdbMapping> GetSceneIgdbMappings(int id)
        {
            _logger.Debug("Fetching Mappings for: {0}", id);

            var request = _xemRequestBuilder.Create()
                                            .Resource("/all")
                                            .AddQueryParam("id", id)
                                            .Build();

            var response = _httpClient.Get<XemResult<List<XemSceneIgdbMapping>>>(request).Resource;

            return response.Data.Where(c => c.Scene != null).ToList();
        }

        public List<SceneMapping> GetSceneIgdbNames()
        {
            _logger.Debug("Fetching alternate names");

            var request = _xemRequestBuilder.Create()
                                            .Resource("/allNames")
                                            .AddQueryParam("platformNumbers", true)
                                            .Build();

            var response = _httpClient.Get<XemResult<Dictionary<int, List<JObject>>>>(request).Resource;

            var result = new List<SceneMapping>();

            foreach (var game in response.Data)
            {
                foreach (var name in game.Value)
                {
                    foreach (var n in name)
                    {
                        if (!int.TryParse(n.Value.ToString(), out var platformNumber))
                        {
                            continue;
                        }

                        // hack to deal with Fate/Zero
                        if (game.Key == 79151 && platformNumber > 1)
                        {
                            continue;
                        }

                        result.Add(new SceneMapping
                                   {
                                       Title = n.Key,
                                       SearchTerm = n.Key,
                                       ScenePlatformNumber = platformNumber,
                                       IgdbId = game.Key
                                   });
                    }
                }
            }

            return result;
        }

        private static void CheckForFailureResult<T>(XemResult<T> response)
        {
            if (response.Result.Equals("failure", StringComparison.InvariantCultureIgnoreCase) &&
                !IgnoredErrors.Any(knowError => response.Message.Contains(knowError)))
            {
                throw new Exception("Error response received from Xem: " + response.Message);
            }
        }
    }
}
