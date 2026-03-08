using Playarr.Core.Configuration;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Settings;

public class MetadataSourceSettingsResource : RestResource
{
    public string? TwitchClientId { get; set; }
    public string? TwitchClientSecret { get; set; }
    public string? RatingSource { get; set; }
}

public static class MetadataSourceSettingsResourceMapper
{
    public static MetadataSourceSettingsResource ToResource(IConfigService model)
    {
        return new MetadataSourceSettingsResource
        {
            TwitchClientId = model.TwitchClientId,
            TwitchClientSecret = model.TwitchClientSecret,
            RatingSource = model.RatingSource
        };
    }
}
