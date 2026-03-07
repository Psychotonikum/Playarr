using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.AutoTagging;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests.GameServiceTests
{
    [TestFixture]
    public class UpdateSeriesFixture : CoreTest<GameService>
    {
        private Game _fakeSeries;
        private Game _existingGame;

        [SetUp]
        public void Setup()
        {
            _fakeSeries = Builder<Game>.CreateNew().Build();
            _existingGame = Builder<Game>.CreateNew().Build();

            _fakeSeries.Platforms = new List<Platform>
            {
                new Platform { SeasonNumber = 1, Monitored = true },
                new Platform { SeasonNumber = 2, Monitored = true }
            };

            _existingGame.Platforms = new List<Platform>
            {
                new Platform { SeasonNumber = 1, Monitored = true },
                new Platform { SeasonNumber = 2, Monitored = true }
            };

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Game>()))
                .Returns(new AutoTaggingChanges());

            Mocker.GetMock<IGameRepository>()
                .Setup(s => s.Update(It.IsAny<Game>()))
                .Returns<Game>(r => r);
        }

        private void GivenExistingSeries()
        {
            Mocker.GetMock<IGameRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(_existingGame);
        }

        [Test]
        public void should_not_update_episodes_if_season_hasnt_changed()
        {
            GivenExistingSeries();

            Subject.UpdateSeries(_fakeSeries);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_update_series_when_it_changes()
        {
            GivenExistingSeries();
            var platformNumber = 1;
            var monitored = false;

            _fakeSeries.Platforms.Single(s => s.SeasonNumber == platformNumber).Monitored = monitored;

            Subject.UpdateSeries(_fakeSeries);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, platformNumber, monitored), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_add_and_remove_tags()
        {
            GivenExistingSeries();
            var platformNumber = 1;
            var monitored = false;

            _fakeSeries.Tags = new HashSet<int> { 1, 2 };
            _fakeSeries.Platforms.Single(s => s.SeasonNumber == platformNumber).Monitored = monitored;

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_fakeSeries))
                .Returns(new AutoTaggingChanges
                {
                    TagsToAdd = new HashSet<int> { 3 },
                    TagsToRemove = new HashSet<int> { 1 }
                });

            var result = Subject.UpdateSeries(_fakeSeries);

            result.Tags.Should().BeEquivalentTo(new[] { 2, 3 });
        }
    }
}
