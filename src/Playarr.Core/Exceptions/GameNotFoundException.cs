using Playarr.Common.Exceptions;

namespace Playarr.Core.Exceptions
{
    public class SeriesNotFoundException : PlayarrException
    {
        public int TvdbGameId { get; set; }

        public SeriesNotFoundException(int tvdbGameId)
            : base(string.Format("Game with tvdbid {0} was not found, it may have been removed from TheIGDB.", tvdbGameId))
        {
            TvdbGameId = tvdbGameId;
        }

        public SeriesNotFoundException(int tvdbGameId, string message, params object[] args)
            : base(message, args)
        {
            TvdbGameId = tvdbGameId;
        }

        public SeriesNotFoundException(int tvdbGameId, string message)
            : base(message)
        {
            TvdbGameId = tvdbGameId;
        }
    }
}
