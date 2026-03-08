using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests.EpisodeMonitoredServiceTests
{
    [TestFixture]
    public class LegacySetEpisodeMontitoredFixture : CoreTest<EpisodeMonitoredService>
    {
        private Game _series;
        private List<Rom> _episodes;

        [SetUp]
        public void Setup()
        {
            var platforms = 4;

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Platforms = Builder<Platform>.CreateListOfSize(platforms)
                                                                           .All()
                                                                           .With(n => n.Monitored = true)
                                                                           .Build()
                                                                           .ToList())
                                     .Build();

            _episodes = Builder<Rom>.CreateListOfSize(platforms)
                                        .All()
                                        .With(e => e.Monitored = true)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-7))

                                        // Missing
                                        .TheFirst(1)
                                        .With(e => e.EpisodeFileId = 0)

                                        // Has File
                                        .TheNext(1)
                                        .With(e => e.EpisodeFileId = 1)

                                         // Future
                                        .TheNext(1)
                                        .With(e => e.EpisodeFileId = 0)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(7))

                                        // Future/TBA
                                        .TheNext(1)
                                        .With(e => e.EpisodeFileId = 0)
                                        .With(e => e.AirDateUtc = null)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenSpecials()
        {
            foreach (var rom in _episodes)
            {
                rom.PlatformNumber = 0;
            }

            _series.Platforms = new List<Platform> { new Platform { Monitored = false, PlatformNumber = 0 } };
        }

        [Test]
        public void should_be_able_to_monitor_series_without_changing_episodes()
        {
            Subject.SetEpisodeMonitoredStatus(_series, null);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.IsAny<List<Rom>>()), Times.Never());
        }

        [Test]
        public void should_be_able_to_monitor_all_episodes()
        {
            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(l => l.All(e => e.Monitored))));
        }

        [Test]
        public void should_be_able_to_monitor_missing_episodes_only()
        {
            var monitoringOptions = new MonitoringOptions
                                    {
                                        IgnoreEpisodesWithFiles = true,
                                        IgnoreEpisodesWithoutFiles = false
                                    };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => !e.HasFile);
            VerifyNotMonitored(e => e.HasFile);
        }

        [Test]
        public void should_be_able_to_monitor_new_episodes_only()
        {
            var monitoringOptions = new MonitoringOptions
            {
                IgnoreEpisodesWithFiles = true,
                IgnoreEpisodesWithoutFiles = true
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.After(DateTime.UtcNow));
            VerifyMonitored(e => !e.AirDateUtc.HasValue);
            VerifyNotMonitored(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow));
        }

        [Test]
        public void should_not_monitor_missing_specials()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                IgnoreEpisodesWithFiles = true,
                IgnoreEpisodesWithoutFiles = false
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_not_monitor_new_specials()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                IgnoreEpisodesWithFiles = true,
                IgnoreEpisodesWithoutFiles = true
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_not_monitor_season_when_all_episodes_are_monitored_except_latest_season()
        {
            _series.Platforms = Builder<Platform>.CreateListOfSize(2)
                                             .All()
                                             .With(n => n.Monitored = true)
                                             .Build()
                                             .ToList();

            _episodes = Builder<Rom>.CreateListOfSize(5)
                                        .All()
                                        .With(e => e.PlatformNumber = 1)
                                        .With(e => e.EpisodeFileId = 0)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-5))
                                        .TheLast(1)
                                        .With(e => e.PlatformNumber = 2)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);

            var monitoringOptions = new MonitoringOptions
            {
                IgnoreEpisodesWithoutFiles = true
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifySeasonMonitored(n => n.PlatformNumber == 2);
            VerifySeasonNotMonitored(n => n.PlatformNumber == 1);
        }

        [Test]
        public void should_ignore_episodes_when_season_is_not_monitored()
        {
            _series.Platforms.ForEach(s => s.Monitored = false);

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(l => l.All(e => !e.Monitored))));
        }

        [Test]
        public void should_should_not_monitor_episodes_if_season_is_not_monitored()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Platforms = Builder<Platform>.CreateListOfSize(2)
                                                                           .TheFirst(1)
                                                                           .With(n => n.Monitored = true)
                                                                           .TheLast(1)
                                                                           .With(n => n.Monitored = false)
                                                                           .Build()
                                                                           .ToList())
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(10)
                                           .All()
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-7))
                                           .TheFirst(5)
                                           .With(e => e.PlatformNumber = 1)
                                           .TheLast(5)
                                           .With(e => e.PlatformNumber = 2)
                                           .BuildList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(roms);

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions
                                                       {
                                                           IgnoreEpisodesWithFiles = true,
                                                           IgnoreEpisodesWithoutFiles = false
                                                       });

            VerifyMonitored(e => e.PlatformNumber == 1);
            VerifyNotMonitored(e => e.PlatformNumber == 2);
            VerifySeasonMonitored(s => s.PlatformNumber == 1);
            VerifySeasonNotMonitored(s => s.PlatformNumber == 2);
        }

        private void VerifyMonitored(Func<Rom, bool> predicate)
        {
            Mocker.GetMock<IRomService>()
                .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(l => l.Where(predicate).All(e => e.Monitored))));
        }

        private void VerifyNotMonitored(Func<Rom, bool> predicate)
        {
            Mocker.GetMock<IRomService>()
                .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(l => l.Where(predicate).All(e => !e.Monitored))));
        }

        private void VerifySeasonMonitored(Func<Platform, bool> predicate)
        {
            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Where(predicate).All(n => n.Monitored)), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        private void VerifySeasonNotMonitored(Func<Platform, bool> predicate)
        {
            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Where(predicate).All(n => !n.Monitored)), It.IsAny<bool>(), It.IsAny<bool>()));
        }
    }
}
