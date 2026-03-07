namespace Playarr.Api.V5.Roms;

public class EpisodesMonitoredResource
{
    public required List<int> RomIds { get; set; }
    public bool Monitored { get; set; }
}
