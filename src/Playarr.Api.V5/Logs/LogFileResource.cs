using Playarr.Http.REST;

namespace Playarr.Api.V5.Logs;

public class LogFileResource : RestResource
{
    public required string Filename { get; set; }
    public required DateTime LastWriteTime { get; set; }
    public required string ContentsUrl { get; set; }
    public required string DownloadUrl { get; set; }
}
