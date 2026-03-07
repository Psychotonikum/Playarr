using Playarr.Core.Games;

namespace Playarr.Api.V5.PlatformPass;

public class PlatformPassResource
{
    public List<PlatformPassGameResource> Game { get; set; } = [];
    public MonitoringOptionsResource? MonitoringOptions { get; set; }
}

public class MonitoringOptionsResource
{
    public MonitorTypes Monitor { get; set; }
}

public static class MonitoringOptionsResourceMapper
{
    public static MonitoringOptions ToModel(this MonitoringOptionsResource? resource)
    {
        if (resource == null)
        {
            return new MonitoringOptions();
        }

        return new MonitoringOptions
        {
            Monitor = resource.Monitor
        };
    }
}
