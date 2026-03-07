using System.Collections.Generic;
using Playarr.Common.Cloud;
using Playarr.Common.Http;

namespace Playarr.Core.DataAugmentation.Scene
{
    public interface ISceneMappingProxy
    {
        List<SceneMapping> Fetch();
    }

    public class SceneMappingProxy : ISceneMappingProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _requestBuilder;

        public SceneMappingProxy(IHttpClient httpClient, IPlayarrCloudRequestBuilder requestBuilder)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder.Services;
        }

        public List<SceneMapping> Fetch()
        {
            var request = _requestBuilder.Create()
                                         .Resource("/scenemapping")
                                         .Build();

            return _httpClient.Get<List<SceneMapping>>(request).Resource;
        }
    }
}
