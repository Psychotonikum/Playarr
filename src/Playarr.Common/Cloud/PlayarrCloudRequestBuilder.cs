using Playarr.Common.Http;

namespace Playarr.Common.Cloud
{
    public interface IPlayarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory SkyHookTvdb { get; }
    }

    public class PlayarrCloudRequestBuilder : IPlayarrCloudRequestBuilder
    {
        public PlayarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://services.playarr.tv/v1/")
                .CreateFactory();

            SkyHookTvdb = new HttpRequestBuilder("https://skyhook.playarr.tv/v1/tvdb/{route}/{language}/")
                .SetSegment("language", "en")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory SkyHookTvdb { get; }
    }
}
