using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class EpisodesWithoutFilesFixture : DbTest<RomRepository, Rom>
    {
        private Game _monitoredSeries;
        private Game _unmonitoredSeries;
        private PagingSpec<Rom> _pagingSpec;

        [SetUp]
        public void Setup()
        {
            _monitoredSeries = Builder<Game>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.MobyGamesId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = true)
                                        .With(s => s.TitleSlug = "Title3")
                                        .Build();

            _unmonitoredSeries = Builder<Game>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.IgdbId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = false)
                                        .With(s => s.TitleSlug = "Title2")
                                        .Build();

            _monitoredSeries.Id = Db.Insert(_monitoredSeries).Id;
            _unmonitoredSeries.Id = Db.Insert(_unmonitoredSeries).Id;

            _pagingSpec = new PagingSpec<Rom>
                              {
                                  Page = 1,
                                  PageSize = 10,
                                  SortKey = "AirDate",
                                  SortDirection = SortDirection.Ascending
                              };

            var monitoredSeriesEpisodes = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.GameId = _monitoredSeries.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .TheLast(1)
                                           .With(e => e.PlatformNumber = 0)
                                           .Build();

            var unmonitoredSeriesEpisodes = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.GameId = _unmonitoredSeries.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .TheLast(1)
                                           .With(e => e.PlatformNumber = 0)
                                           .Build();

            var unairedEpisodes           = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.GameId = _monitoredSeries.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .Build();

            Db.InsertMany(monitoredSeriesEpisodes);
            Db.InsertMany(unmonitoredSeriesEpisodes);
            Db.InsertMany(unairedEpisodes);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == true && e.Game.Monitored == true);
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == false || e.Game.Monitored == false);
        }

        [Test]
        public void should_get_monitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var roms = Subject.EpisodesWithoutFiles(_pagingSpec, false);

            roms.Records.Should().HaveCount(1);
        }

        [Test]
        [Ignore("Specials not implemented")]
        public void should_get_episode_including_specials()
        {
            var roms = Subject.EpisodesWithoutFiles(_pagingSpec, true);

            roms.Records.Should().HaveCount(2);
        }

        [Test]
        public void should_not_include_unmonitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var roms = Subject.EpisodesWithoutFiles(_pagingSpec, false);

            roms.Records.Should().NotContain(e => e.Monitored == false);
        }

        [Test]
        public void should_not_contain_unmonitored_series()
        {
            GivenMonitoredFilterExpression();

            var roms = Subject.EpisodesWithoutFiles(_pagingSpec, false);

            roms.Records.Should().NotContain(e => e.GameId == _unmonitoredSeries.Id);
        }

        [Test]
        public void should_not_return_unaired()
        {
            var roms = Subject.EpisodesWithoutFiles(_pagingSpec, false);

            roms.TotalRecords.Should().Be(4);
        }

        [Test]
        public void should_not_return_episodes_on_air()
        {
            var onAirEpisode = Builder<Rom>.CreateNew()
                                               .With(e => e.Id = 0)
                                               .With(e => e.GameId = _monitoredSeries.Id)
                                               .With(e => e.EpisodeFileId = 0)
                                               .With(e => e.AirDateUtc = DateTime.Now.AddMinutes(-15))
                                               .With(e => e.Monitored = true)
                                               .Build();

            Db.Insert(onAirEpisode);

            var roms = Subject.EpisodesWithoutFiles(_pagingSpec, false);

            roms.TotalRecords.Should().Be(4);
            roms.Records.Where(e => e.Id == onAirEpisode.Id).Should().BeEmpty();
        }
    }
}
