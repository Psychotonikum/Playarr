using Playarr.Api.V5.Game;

namespace Playarr.Api.V5.PlatformPass;

public class PlatformPassGameResource
{
    public int Id { get; set; }
    public bool? Monitored { get; set; }
    public List<SeasonResource> Platforms { get; set; } = [];
}
