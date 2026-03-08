using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class IdFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                      .CreateNew()
                      .With(s => s.Title = "Game Title")
                      .With(s => s.ImdbId = "tt12345")
                      .With(s => s.IgdbId = 12345)
                      .With(s => s.MobyGamesId = 54321)
                      .Build();

            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_add_imdb_id()
        {
            _namingConfig.GameFolderFormat = "{Game Title} ({ImdbId})";

            Subject.GetGameFolder(_series)
                   .Should().Be($"Game Title ({_series.ImdbId})");
        }

        [Test]
        public void should_add_igdb_id()
        {
            _namingConfig.GameFolderFormat = "{Game Title} ({IgdbId})";

            Subject.GetGameFolder(_series)
                   .Should().Be($"Game Title ({_series.IgdbId})");
        }

        [Test]
        public void should_add_tvmaze_id()
        {
            _namingConfig.GameFolderFormat = "{Game Title} ({RawgId})";

            Subject.GetGameFolder(_series)
                   .Should().Be($"Game Title ({_series.RawgId})");
        }

        [Test]
        public void should_add_tmdb_id()
        {
            _namingConfig.GameFolderFormat = "{Game Title} ({TmdbId})";

            Subject.GetGameFolder(_series)
                .Should().Be($"Game Title ({_series.TmdbId})");
        }
    }
}
