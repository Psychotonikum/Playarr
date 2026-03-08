using Playarr.Common.Http;

namespace Playarr.Common.Cloud
{
    public interface IPlayarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory SkyHookIgdb { get; }
    }

    public class PlayarrCloudRequestBuilder : IPlayarrCloudRequestBuilder
    {
        public PlayarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://services.playarr.tv/v1/")
                .CreateFactory();

            SkyHookIgdb = new HttpRequestBuilder("https://skyhook.playarr.tv/v1/igdb/{route}/{language}/")
                .SetSegment("language", "en")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory SkyHookIgdb { get; }
    }
}
