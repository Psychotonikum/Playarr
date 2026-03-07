using System.Collections.Generic;
using Playarr.Core.Datastore;

namespace Playarr.Core.Games
{
    public class Platform : IEmbeddedDocument
    {
        public Platform()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
