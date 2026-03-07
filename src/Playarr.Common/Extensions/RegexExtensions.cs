using System.Text.RegularExpressions;

namespace Playarr.Common.Extensions
{
    public static class RegexExtensions
    {
        public static int EndIndex(this Capture regexMatch)
        {
            return regexMatch.Index + regexMatch.Length;
        }
    }
}
