using System.Collections.Generic;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Games
{
    public class MultipleSeriesFoundException : PlayarrException
    {
        public List<Game> Game { get; set; }

        public MultipleSeriesFoundException(List<Game> game, string message, params object[] args)
            : base(message, args)
        {
            Game = game;
        }
    }
}
