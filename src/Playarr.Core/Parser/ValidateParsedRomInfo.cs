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
            return true;
        }
    }
}
