using NLog;
using Playarr.Common.Instrumentation;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Parser
{
    public static class ValidateParsedRomInfo
    {
        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(ValidateParsedRomInfo));

        public static bool ValidateForGameType(ParsedRomInfo parsedRomInfo, Game game, bool warnIfInvalid = true)
        {
            if (parsedRomInfo.IsDaily && game.SeriesType == GameTypes.Standard)
            {
                var message = $"Found daily-style rom for non-daily game: {game}";

                if (warnIfInvalid)
                {
                    Logger.Warn(message);
                }
                else
                {
                    Logger.Debug(message);
                }

                return false;
            }

            return true;
        }
    }
}
