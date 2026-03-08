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
    public class SetEpisodeMontitoredFixture : CoreTest<EpisodeMonitoredService>
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
                                        Monitor = MonitorTypes.Missing
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
                Monitor = MonitorTypes.Future
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
                Monitor = MonitorTypes.Missing
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
                Monitor = MonitorTypes.Future
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_monitor_specials()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.MonitorSpecials
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_unmonitor_specials()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.UnmonitorSpecials
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_unmonitor_specials_after_monitoring()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.MonitorSpecials
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.UnmonitorSpecials
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_not_monitor_season_when_all_episodes_are_monitored_except_last_season()
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
                Monitor = MonitorTypes.LastSeason
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifySeasonMonitored(n => n.PlatformNumber == 2);
            VerifySeasonNotMonitored(n => n.PlatformNumber == 1);
        }

        [Test]
        public void should_be_able_to_monitor_no_episodes()
        {
            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.None
                                    };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(l => l.All(e => !e.Monitored))));
        }

        [Test]
        public void should_monitor_missing_episodes()
        {
            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.Missing
                                    };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => !e.HasFile);
            VerifyNotMonitored(e => e.HasFile);
        }

        [Test]
        public void should_monitor_last_season_if_all_episodes_aired_more_than_90_days_ago()
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
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-200))
                .TheLast(2)
                .With(e => e.PlatformNumber = 2)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-100))
                .Build()
                .ToList();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.LastSeason
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifySeasonMonitored(n => n.PlatformNumber == 2);
            VerifyMonitored(n => n.PlatformNumber == 2);

            VerifySeasonNotMonitored(n => n.PlatformNumber == 1);
            VerifyNotMonitored(n => n.PlatformNumber == 1);
        }

        [Test]
        public void should_not_monitor_any_recent_episodes_if_all_episodes_aired_more_than_90_days_ago()
        {
            _episodes.ForEach(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-100));

            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.Recent
                                    };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateEpisodes(It.Is<List<Rom>>(l => l.All(e => !e.Monitored))));
        }

        [Test]
        public void should_not_monitor_last_season_for_future_episodes_if_all_episodes_already_aired()
        {
            _episodes.ForEach(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-7));

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Future
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifySeasonNotMonitored(n => n.PlatformNumber > 0);
            VerifyNotMonitored(n => n.PlatformNumber > 0);
        }

        [Test]
        public void should_monitor_any_recent_and_future_episodes_if_all_episodes_aired_within_90_days()
        {
            _series.Platforms = Builder<Platform>.CreateListOfSize(1)
                .All()
                .With(n => n.Monitored = true)
                .Build()
                .ToList();

            _episodes = Builder<Rom>.CreateListOfSize(5)
                .All()
                .With(e => e.PlatformNumber = 1)
                .With(e => e.EpisodeFileId = 0)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-200))
                .TheLast(3)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-5))
                .TheLast(1)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(30))
                .Build()
                .ToList();

            Mocker.GetMock<IRomService>()
                .Setup(s => s.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(_episodes);

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Recent
            };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifySeasonMonitored(n => n.PlatformNumber == 1);
            VerifyNotMonitored(n => n.AirDateUtc.HasValue && n.AirDateUtc.Value.Before(DateTime.UtcNow.AddDays(-90)));
            VerifyMonitored(n => n.AirDateUtc.HasValue && n.AirDateUtc.Value.After(DateTime.UtcNow.AddDays(-90)));
        }

        [Test]
        public void should_monitor_latest_season_if_some_episodes_have_aired()
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
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-100))
                                        .TheLast(2)
                                        .With(e => e.PlatformNumber = 2)
                                        .TheLast(1)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(100))
                                        .Build()
                                        .ToList();

            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.LastSeason
                                    };

            Subject.SetEpisodeMonitoredStatus(_series, monitoringOptions);

            VerifySeasonMonitored(n => n.PlatformNumber == 2);
            VerifyMonitored(n => n.PlatformNumber == 2);

            VerifySeasonNotMonitored(n => n.PlatformNumber == 1);
            VerifyNotMonitored(n => n.PlatformNumber == 1);
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
