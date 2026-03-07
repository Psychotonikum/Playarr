using Playarr.Core.CustomFormats;
using Playarr.Core.Datastore;

namespace Playarr.Core.Profiles
{
    public class ProfileFormatItem : IEmbeddedDocument
    {
        public CustomFormat Format { get; set; }
        public int Score { get; set; }
    }
}
