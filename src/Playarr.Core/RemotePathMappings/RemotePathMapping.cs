using Playarr.Core.Datastore;

namespace Playarr.Core.RemotePathMappings
{
    public class RemotePathMapping : ModelBase
    {
        public string Host { get; set; }
        public string RemotePath { get; set; }
        public string LocalPath { get; set; }
    }
}
