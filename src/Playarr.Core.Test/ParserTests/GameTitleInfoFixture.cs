using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class GameTitleInfoFixture : CoreTest
    {
        [Test]
        public void should_have_year_zero_when_title_doesnt_have_a_year()
        {
            const string title = "Game.Title.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).GameTitleInfo;

            result.Year.Should().Be(0);
        }

        [Test]
        public void should_have_same_title_for_title_and_title_without_year_when_title_doesnt_have_a_year()
        {
            const string title = "Game.Title.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).GameTitleInfo;

            result.Title.Should().Be(result.TitleWithoutYear);
        }

        [Test]
        public void should_have_year_when_title_has_a_year()
        {
            const string title = "Game.Title.2004.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).GameTitleInfo;

            result.Year.Should().Be(2004);
        }

        [Test]
        public void should_have_year_in_title_when_title_has_a_year()
        {
            const string title = "Game.Title.2004.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).GameTitleInfo;

            result.Title.Should().Be("Game Title 2004");
        }

        [Test]
        public void should_title_without_year_should_not_contain_year()
        {
            const string title = "Game.Title.2004.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).GameTitleInfo;

            result.TitleWithoutYear.Should().Be("Game Title");
        }
    }
}
