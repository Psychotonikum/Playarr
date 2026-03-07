using System;
using Playarr.Core.Backup;
using Playarr.Http.REST;

namespace Playarr.Api.V3.System.Backup
{
    public class BackupResource : RestResource
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BackupType Type { get; set; }
        public long Size { get; set; }
        public DateTime Time { get; set; }
    }
}
