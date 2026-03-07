using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Parser;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetSeriesFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "30 Stone";

            Subject.GetSeries(title);

            Mocker.GetMock<IGameService>()
                  .Verify(s => s.FindByTitle(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_series_title()
        {
            const string title = "30.Stone.S01E01.720p.hdtv";

            Subject.GetSeries(title);

            Mocker.GetMock<IGameService>()
                  .Verify(s => s.FindByTitle(Parser.Parser.ParseTitle(title).GameTitle), Times.Once());
        }

        [Test]
        public void should_fallback_to_title_without_year_and_year_when_title_lookup_fails()
        {
            const string title = "Show.2004.S01E01.720p.hdtv";
            var parsedRomInfo = Parser.Parser.ParseTitle(title);

            Subject.GetSeries(title);

            Mocker.GetMock<IGameService>()
                  .Verify(s => s.FindByTitle(parsedRomInfo.GameTitleInfo.TitleWithoutYear,
                                             parsedRomInfo.GameTitleInfo.Year),
                      Times.Once());
        }

        [Test]
        public void should_parse_concatenated_title()
        {
            var game = new Game { TvdbId = 100 };
            Mocker.GetMock<IGameService>().Setup(v => v.FindByTitle("Welcome")).Returns(game);
            Mocker.GetMock<ISceneMappingService>().Setup(v => v.FindIgdbId("Mairimashita", It.IsAny<string>(), It.IsAny<int>())).Returns(100);

            var result = Subject.GetSeries("Welcome (Mairimashita).S01E01.720p.WEB-DL-Viva");

            result.Should().NotBeNull();
            result.TvdbId.Should().Be(100);
        }
    }
}
