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

        [TestCase("The Simpsons", "The Simpsons")]
        [TestCase("South Park", "South Park")]
        [TestCase("Franklin & Bash", "Franklin & Bash")]
        [TestCase("House", "House")]
        [TestCase("Mr. D", "Mr. D")]
        [TestCase("Rob & Big", "Rob & Big")]
        [TestCase("M*A*S*H", "M*A*S*H")]

        // [TestCase("imdb:tt0436992", "Doctor Who (2005)")]
        [TestCase("tvdb:78804", "Doctor Who (2005)")]
        [TestCase("tvdbid:78804", "Doctor Who (2005)")]
        [TestCase("tvdbid: 78804 ", "Doctor Who (2005)")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewSeries(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tt0496424", "30 Rock")]
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

        [TestCase("tvdbid:")]
        [TestCase("tvdbid: 99999999999999999999")]
        [TestCase("tvdbid: 0")]
        [TestCase("tvdbid: -12")]
        [TestCase("tvdbid:289578")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewSeries(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tvdbid:78804")]
        [TestCase("Doctor Who")]
        public void should_return_existing_series_if_found(string term)
        {
            const int igdbId = 78804;
            var existingGame = new Game
            {
                TvdbId = igdbId
            };

            Mocker.GetMock<IGameService>().Setup(c => c.FindByIgdbId(igdbId)).Returns(existingGame);

            var result = Subject.SearchForNewSeries("tvdbid: " + igdbId);

            result.Should().Contain(existingGame);
            result.Should().ContainSingle(c => c.TvdbId == igdbId);
        }
    }
}
