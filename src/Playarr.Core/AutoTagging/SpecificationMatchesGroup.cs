using System.Collections.Generic;
using System.Linq;
using Playarr.Core.AutoTagging.Specifications;

namespace Playarr.Core.AutoTagging
{
    public class SpecificationMatchesGroup
    {
        public Dictionary<IAutoTaggingSpecification, bool> Matches { get; set; }

        public bool DidMatch => !(Matches.Any(m => m.Key.Required && m.Value == false) ||
                                  Matches.All(m => m.Value == false));
    }
}
