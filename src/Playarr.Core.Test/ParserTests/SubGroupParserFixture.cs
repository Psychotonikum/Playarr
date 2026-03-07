using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class SubGroupParserFixture : CoreTest
    {
        [TestCase("[GHOST][1080p] Game - 25 [BD HEVC 10bit Dual Audio AC3][AE0ADDBA]", "GHOST")]
        public void should_parse_sub_group_from_title_as_release_group(string title, string expected)
        {
            var result = Parser.Parser.ParseTitle(title);

            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(expected);
        }
    }
}
