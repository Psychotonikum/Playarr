using System.Collections.Generic;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class SceneEpisodeMapping
    {
        public Rom Rom { get; set; }
        public SearchMode SearchMode { get; set; }
        public List<string> SceneTitles { get; set; }
        public int PlatformNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }

        public override int GetHashCode()
        {
            return SearchMode.GetHashCode() ^ PlatformNumber.GetHashCode() ^ EpisodeNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as SceneEpisodeMapping;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return PlatformNumber == other.PlatformNumber && EpisodeNumber == other.EpisodeNumber && SearchMode == other.SearchMode;
        }
    }
}
