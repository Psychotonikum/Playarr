using Newtonsoft.Json;
using Playarr.Core.Datastore;

namespace Playarr.Core.DataAugmentation.Scene
{
    public class SceneMapping : ModelBase
    {
        public string Title { get; set; }
        public string ParseTerm { get; set; }

        [JsonProperty("searchTitle")]
        public string SearchTerm { get; set; }

        public int IgdbId { get; set; }

        [JsonProperty("platform")]
        public int? PlatformNumber { get; set; }

        public int? ScenePlatformNumber { get; set; }

        public string SceneOrigin { get; set; }
        public SearchMode? SearchMode { get; set; }
        public string Comment { get; set; }

        public string FilterRegex { get; set; }

        public string Type { get; set; }
    }
}
