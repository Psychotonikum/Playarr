using Playarr.Core.Backup;
using Playarr.Http.REST;

namespace Playarr.Api.V5.System.Backup;

public class BackupResource : RestResource
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public BackupType Type { get; set; }
    public long Size { get; set; }
    public DateTime Time { get; set; }
}
