using Playarr.Core.Games;

namespace Playarr.Core.Parser.Model
{
    public class FindSeriesResult
    {
        public Game Game { get; set; }
        public SeriesMatchType MatchType { get; set; }

        public FindSeriesResult(Game game, SeriesMatchType matchType)
        {
            Game = game;
            MatchType = matchType;
        }
    }

    public enum SeriesMatchType
    {
        Unknown = 0,
        Title = 1,
        Alias = 2,
        Id = 3
    }
}
