using Playarr.Core.MediaCover;
using Playarr.Core.Games;

namespace Playarr.Api.V5.Game;

public class SeasonResource
{
    public int PlatformNumber { get; set; }
    public bool Monitored { get; set; }
    public SeasonStatisticsResource? Statistics { get; set; }
    public List<MediaCover>? Images { get; set; }
}

public static class SeasonResourceMapper
{
    public static SeasonResource ToResource(this Platform model, bool includeImages = false)
    {
        return new SeasonResource
        {
            PlatformNumber = model.PlatformNumber,
            Monitored = model.Monitored,
            Images = includeImages ? model.Images : null
        };
    }

    public static Platform ToModel(this SeasonResource resource)
    {
        return new Platform
        {
            PlatformNumber = resource.PlatformNumber,
            Monitored = resource.Monitored
        };
    }

    public static List<SeasonResource> ToResource(this IEnumerable<Platform> models, bool includeImages = false)
    {
        return models.Select(s => ToResource(s, includeImages)).ToList();
    }

    public static List<Platform> ToModel(this IEnumerable<SeasonResource> resources)
    {
        return resources.Select(ToModel).ToList();
    }
}
