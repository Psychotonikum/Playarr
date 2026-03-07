using System.Collections.Generic;
using Playarr.Core.Datastore;

namespace Playarr.Core.Games
{
    public class Actor : IEmbeddedDocument
    {
        public Actor()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }
        public string Character { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
