using System.Net;

namespace Playarr.Common.Http
{
    public class BasicNetworkCredential : NetworkCredential
    {
        public BasicNetworkCredential(string user, string pass)
        : base(user, pass)
        {
        }
    }
}
