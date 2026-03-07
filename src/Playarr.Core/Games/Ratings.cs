using Playarr.Core.Datastore;

namespace Playarr.Core.Games
{
    public class Ratings : IEmbeddedDocument
    {
        public int Votes { get; set; }
        public decimal Value { get; set; }
    }
}
