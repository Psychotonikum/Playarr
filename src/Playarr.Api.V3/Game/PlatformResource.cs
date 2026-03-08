using System.Collections.Generic;
using System.Linq;
using Playarr.Core.MediaCover;
using Playarr.Core.Games;

namespace Playarr.Api.V3.Game
{
    public class SeasonResource
    {
        public int PlatformNumber { get; set; }
        public string Title { get; set; }
        public bool Monitored { get; set; }
        public SeasonStatisticsResource Statistics { get; set; }
        public List<MediaCover> Images { get; set; }
    }

    public static class SeasonResourceMapper
    {
        public static SeasonResource ToResource(this Platform model, bool includeImages = false)
        {
            if (model == null)
            {
                return null;
            }

            return new SeasonResource
            {
                PlatformNumber = model.PlatformNumber,
                Title = model.Title,
                Monitored = model.Monitored,
                Images = includeImages ? model.Images : null
            };
        }

        public static Platform ToModel(this SeasonResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Platform
            {
                PlatformNumber = resource.PlatformNumber,
                Title = resource.Title,
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
}
