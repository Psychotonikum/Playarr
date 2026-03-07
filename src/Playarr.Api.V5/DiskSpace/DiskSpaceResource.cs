using Playarr.Http.REST;

namespace Playarr.Api.V5.DiskSpace;

public class DiskSpaceResource : RestResource
{
    public required string Path { get; set; }
    public required string Label { get; set; }
    public long FreeSpace { get; set; }
    public long TotalSpace { get; set; }
}

public static class DiskSpaceResourceMapper
{
    public static DiskSpaceResource MapToResource(this Playarr.Core.DiskSpace.DiskSpace model)
    {
        return new DiskSpaceResource
        {
            Path = model.Path,
            Label = model.Label,
            FreeSpace = model.FreeSpace,
            TotalSpace = model.TotalSpace
        };
    }
}
