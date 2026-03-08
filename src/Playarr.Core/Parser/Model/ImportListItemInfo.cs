using System;
using System.Collections.Generic;
using Playarr.Core.Datastore;
using Playarr.Core.Games;

namespace Playarr.Core.Parser.Model
{
    public class ImportListItemInfo : ModelBase
    {
        public ImportListItemInfo()
        {
            Platforms = new List<Platform>();
        }

        public int ImportListId { get; set; }
        public string ImportList { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int IgdbId { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public int MalId { get; set; }
        public int AniListId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<Platform> Platforms { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", ReleaseDate, Title);
        }
    }
}
