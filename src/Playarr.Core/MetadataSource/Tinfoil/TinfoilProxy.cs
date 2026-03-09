using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using NLog;
using Playarr.Common.Http;

namespace Playarr.Core.MetadataSource.Tinfoil
{
    public interface ITinfoilProxy
    {
        List<TinfoilTitle> GetTitlesForGame(string gameTitle);
        List<TinfoilTitle> GetTitlesByTitleId(string titleId);
    }

    public class TinfoilTitle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Region { get; set; }
        public string Type { get; set; } // "Base", "Update", "DLC"
        public long? Size { get; set; }
        public string ReleaseDate { get; set; }
        public string Publisher { get; set; }
    }

    public class TinfoilProxy : ITinfoilProxy
    {
        private const string TinfoilApiBase = "https://tinfoil.io/Title/ApiJson/";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private Dictionary<string, TinfoilTitle> _titleCache;
        private DateTime _cacheExpiry;

        public TinfoilProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _titleCache = new Dictionary<string, TinfoilTitle>();
            _cacheExpiry = DateTime.MinValue;
        }

        public List<TinfoilTitle> GetTitlesForGame(string gameTitle)
        {
            try
            {
                EnsureCacheLoaded();

                var normalizedSearch = gameTitle.ToLowerInvariant().Trim();

                return _titleCache.Values
                    .Where(t => t.Name != null &&
                                t.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.Type == "Base" ? 0 : t.Type == "Update" ? 1 : 2)
                    .ThenBy(t => t.ReleaseDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to search tinfoil titles for: {0}", gameTitle);
                return new List<TinfoilTitle>();
            }
        }

        public List<TinfoilTitle> GetTitlesByTitleId(string titleId)
        {
            try
            {
                EnsureCacheLoaded();

                // Switch title IDs share a base: the last 3 hex digits encode type
                // Base: xxx000, Update: xxx800, DLC: xxx001-xxx7FF
                var baseTitleId = GetBaseTitleId(titleId);

                return _titleCache.Values
                    .Where(t => t.Id != null && GetBaseTitleId(t.Id) == baseTitleId)
                    .OrderBy(t => t.Type == "Base" ? 0 : t.Type == "Update" ? 1 : 2)
                    .ThenBy(t => t.ReleaseDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch tinfoil titles for ID: {0}", titleId);
                return new List<TinfoilTitle>();
            }
        }

        private void EnsureCacheLoaded()
        {
            if (_titleCache.Count > 0 && DateTime.UtcNow < _cacheExpiry)
            {
                return;
            }

            _logger.Info("Loading Tinfoil title database...");

            var request = new HttpRequest(TinfoilApiBase)
            {
                AllowAutoRedirect = true,
                RequestTimeout = TimeSpan.FromSeconds(30)
            };

            request.Headers.Add("User-Agent", "Playarr/1.0");

            var response = _httpClient.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn("Tinfoil API returned status {0}", response.StatusCode);
                return;
            }

            var newCache = new Dictionary<string, TinfoilTitle>();

            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in root.EnumerateObject())
                {
                    var titleId = prop.Name;

                    if (prop.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var title = new TinfoilTitle
                    {
                        Id = titleId,
                        Name = GetStringProp(prop.Value, "name"),
                        Version = GetStringProp(prop.Value, "version"),
                        Region = GetStringProp(prop.Value, "region"),
                        Publisher = GetStringProp(prop.Value, "publisher"),
                        ReleaseDate = GetStringProp(prop.Value, "releaseDate"),
                        Type = ClassifyTitleType(titleId),
                    };

                    if (prop.Value.TryGetProperty("size", out var sizeEl) && sizeEl.ValueKind == JsonValueKind.Number)
                    {
                        title.Size = sizeEl.GetInt64();
                    }

                    newCache[titleId] = title;
                }
            }

            _titleCache = newCache;
            _cacheExpiry = DateTime.UtcNow.AddHours(12);
            _logger.Info("Loaded {0} titles from Tinfoil database", _titleCache.Count);
        }

        private static string ClassifyTitleType(string titleId)
        {
            if (string.IsNullOrEmpty(titleId) || titleId.Length < 4)
            {
                return "Base";
            }

            var suffix = titleId[^3..].ToLowerInvariant();

            if (suffix == "000")
            {
                return "Base";
            }

            if (suffix == "800")
            {
                return "Update";
            }

            return "DLC";
        }

        private static string GetBaseTitleId(string titleId)
        {
            if (string.IsNullOrEmpty(titleId) || titleId.Length < 4)
            {
                return titleId ?? string.Empty;
            }

            return titleId[..^3].ToLowerInvariant();
        }

        private static string GetStringProp(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
    }
}
