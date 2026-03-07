using System.Collections.Generic;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Profiles.Languages
{
    public class LanguageProfileResource : RestResource
    {
        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public Playarr.Core.Languages.Language Cutoff { get; set; }
        public List<LanguageProfileItemResource> Languages { get; set; }
    }

    public class LanguageProfileItemResource : RestResource
    {
        public Playarr.Core.Languages.Language Language { get; set; }
        public bool Allowed { get; set; }
    }
}
