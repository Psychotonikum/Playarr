using Playarr.Core.Configuration;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Config
{
    public class MetadataSourceConfigResource : RestResource
    {
        public string TwitchClientId { get; set; }
        public string TwitchClientSecret { get; set; }
        public string RatingSource { get; set; }
    }

    public static class MetadataSourceConfigResourceMapper
    {
        public static MetadataSourceConfigResource ToResource(IConfigService model)
        {
            return new MetadataSourceConfigResource
            {
                TwitchClientId = model.TwitchClientId,
                TwitchClientSecret = model.TwitchClientSecret,
                RatingSource = model.RatingSource
            };
        }
    }
}
