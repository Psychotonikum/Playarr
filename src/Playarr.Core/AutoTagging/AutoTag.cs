using System.Collections.Generic;
using Playarr.Core.AutoTagging.Specifications;
using Playarr.Core.Datastore;

namespace Playarr.Core.AutoTagging
{
    public class AutoTag : ModelBase
    {
        public AutoTag()
        {
            Tags = new HashSet<int>();
        }

        public string Name { get; set; }
        public List<IAutoTaggingSpecification> Specifications { get; set; }
        public bool RemoveTagsAutomatically { get; set; }
        public HashSet<int> Tags { get; set; }
    }
}
