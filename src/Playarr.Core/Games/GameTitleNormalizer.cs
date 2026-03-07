using System.Collections.Generic;

namespace Playarr.Core.Games
{
    public static class GameTitleNormalizer
    {
        private static readonly Dictionary<int, string> PreComputedTitles = new()
        {
            { 281588, "a to z" },
        };

        public static string Normalize(string title, int igdbId)
        {
            if (PreComputedTitles.TryGetValue(igdbId, out var value))
            {
                return value;
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
