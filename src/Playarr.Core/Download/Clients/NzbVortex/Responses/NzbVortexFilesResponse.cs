using System.Collections.Generic;

namespace Playarr.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexFilesResponse : NzbVortexResponseBase
    {
        public List<NzbVortexFile> Files { get; set; }
    }
}
