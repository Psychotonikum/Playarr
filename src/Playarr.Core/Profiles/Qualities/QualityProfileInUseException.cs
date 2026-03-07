using System.Net;
using Playarr.Core.Exceptions;

namespace Playarr.Core.Profiles.Qualities
{
    public class QualityProfileInUseException : PlayarrClientException
    {
        public QualityProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "QualityProfile [{0}] is in use.", name)
        {
        }
    }
}
