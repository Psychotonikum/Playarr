using Playarr.Common.Exceptions;

namespace Playarr.Core.Exceptions
{
    public class SeriesNotFoundException : PlayarrException
    {
        public int IgdbGameId { get; set; }

        public SeriesNotFoundException(int igdbGameId)
            : base(string.Format("Game with igdbid {0} was not found, it may have been removed from TheIGDB.", igdbGameId))
        {
            IgdbGameId = igdbGameId;
        }

        public SeriesNotFoundException(int igdbGameId, string message, params object[] args)
            : base(message, args)
        {
            IgdbGameId = igdbGameId;
        }

        public SeriesNotFoundException(int igdbGameId, string message)
            : base(message)
        {
            IgdbGameId = igdbGameId;
        }
    }
}
