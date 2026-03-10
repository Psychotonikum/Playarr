using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Localization;

namespace Playarr.Core.ImportLists.Playarr
{
    public interface IPlayarrV3Proxy
    {
        List<PlayarrSeries> GetGame(PlayarrSettings settings);
        List<PlayarrProfile> GetQualityProfiles(PlayarrSettings settings);
        List<PlayarrProfile> GetLanguageProfiles(PlayarrSettings settings);
        List<PlayarrRootFolder> GetRootFolders(PlayarrSettings settings);
        List<PlayarrTag> GetTags(PlayarrSettings settings);
        ValidationFailure Test(PlayarrSettings settings);
    }

    public class PlayarrV3Proxy : IPlayarrV3Proxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ILocalizationService _localizationService;

        public PlayarrV3Proxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public List<PlayarrSeries> GetGame(PlayarrSettings settings)
        {
            return Execute<PlayarrSeries>("/api/v3/game", settings);
        }

        public List<PlayarrProfile> GetQualityProfiles(PlayarrSettings settings)
        {
            return Execute<PlayarrProfile>("/api/v3/qualityprofile", settings);
        }

        public List<PlayarrProfile> GetLanguageProfiles(PlayarrSettings settings)
        {
            return Execute<PlayarrProfile>("/api/v3/languageprofile", settings);
        }

        public List<PlayarrRootFolder> GetRootFolders(PlayarrSettings settings)
        {
            return Execute<PlayarrRootFolder>("api/v3/rootfolder", settings);
        }

        public List<PlayarrTag> GetTags(PlayarrSettings settings)
        {
            return Execute<PlayarrTag>("/api/v3/tag", settings);
        }

        public ValidationFailure Test(PlayarrSettings settings)
        {
            try
            {
                GetGame(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API Key is invalid");
                    return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("ImportListsValidationInvalidApiKey"));
                }

                if (ex.Response.HasHttpRedirect)
                {
                    _logger.Error(ex, "Playarr returned redirect and is invalid");
                    return new ValidationFailure("BaseUrl", _localizationService.GetLocalizedString("ImportListsPlayarrValidationInvalidUrl"));
                }

                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private List<TResource> Execute<TResource>(string resource, PlayarrSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace() || settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var baseUrl = settings.BaseUrl.TrimEnd('/');

            var request = new HttpRequestBuilder(baseUrl).Resource(resource)
                .Accept(HttpAccept.Json)
                .SetHeader("X-Api-Key", settings.ApiKey)
                .Build();

            var response = _httpClient.Get(request);

            if ((int)response.StatusCode >= 300)
            {
                throw new HttpException(response);
            }

            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
