using System.Collections.Generic;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch.Definitions
{
    public class SceneSeasonMapping
    {
        public List<Rom> Roms { get; set; }
        public SceneEpisodeMapping EpisodeMapping { get; set; }
        public SearchMode SearchMode { get; set; }
        public List<string> SceneTitles { get; set; }
        public int SeasonNumber { get; set; }

        public override int GetHashCode()
        {
            return SearchMode.GetHashCode() ^ SeasonNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as SceneSeasonMapping;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return SeasonNumber == other.SeasonNumber && SearchMode == other.SearchMode;
        }
    }
}
