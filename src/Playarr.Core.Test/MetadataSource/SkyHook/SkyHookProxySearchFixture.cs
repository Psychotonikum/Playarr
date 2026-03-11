using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.MetadataSource;
using Playarr.Core.MetadataSource.SkyHook;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;
using Playarr.Test.Common.Categories;

namespace Playarr.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxySearchFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase("The Witcher 3", "The Witcher 3: Wild Hunt")]
        [TestCase("Portal 2", "Portal 2")]
        [TestCase("Grand Theft Auto V", "Grand Theft Auto V")]
        [TestCase("Final Fantasy VII", "Final Fantasy VII")]
        [TestCase("igdb:1942", "The Witcher 3: Wild Hunt")]
        [TestCase("igdbid:1942", "The Witcher 3: Wild Hunt")]
        [TestCase("igdbid: 1942 ", "The Witcher 3: Wild Hunt")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewSeries(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tt0496424")]
        [Ignore("IMDB search not supported for games")]
        public void should_search_by_imdb(string title, string expected)
        {
            var result = Subject.SearchForNewSeriesByImdbId(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("4565se")]
        public void should_not_search_by_imdb_if_invalid(string title)
        {
            var result = Subject.SearchForNewSeriesByImdbId(title);
            result.Should().BeEmpty();

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeries(It.IsAny<string>()), Times.Never());

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("igdbid:")]
        [TestCase("igdbid: 99999999999999999999")]
        [TestCase("igdbid: 0")]
        [TestCase("igdbid: -12")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewSeries(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("igdbid:1942")]
        [TestCase("The Witcher 3")]
        public void should_return_existing_series_if_found(string term)
        {
            const int igdbId = 1942;
            var existingGame = new Game
            {
                IgdbId = igdbId
            };

            Mocker.GetMock<IGameService>().Setup(c => c.FindByIgdbId(igdbId)).Returns(existingGame);

            var result = Subject.SearchForNewSeries("igdbid: " + igdbId);

            result.Should().Contain(existingGame);
            result.Should().ContainSingle(c => c.IgdbId == igdbId);
        }
    }
}
