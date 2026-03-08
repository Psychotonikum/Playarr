using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AcceptableSizeSpecificationFixture : CoreTest<AcceptableSizeSpecification>
    {
        private RemoteEpisode _parseResultMultiSet;
        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Game _series;
        private List<Rom> _episodes;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                    .With(s => s.Platforms = Builder<Platform>.CreateListOfSize(2).Build().ToList())
                                    .With(c => c.QualityProfile = (LazyLoaded<QualityProfile>)new QualityProfile
                                    {
                                        Cutoff = Quality.SDTV.Id, Items = new List<QualityProfileQualityItem>
                                        {
                                            new QualityProfileQualityItem
                                            {
                                                Quality = Quality.SDTV,
                                                MinSize = 2,
                                                MaxSize = 10
                                            },
                                            new QualityProfileQualityItem
                                            {
                                                Quality = Quality.RAWHD,
                                                MinSize = 2,
                                                MaxSize = null
                                            },
                                            new QualityProfileQualityItem
                                            {
                                                Name = "WEB 720p",
                                                Items = new List<QualityProfileQualityItem>
                                                {
                                                    new QualityProfileQualityItem
                                                    {
                                                        Quality = Quality.WEBDL720p,
                                                        MinSize = 2,
                                                        MaxSize = 20,
                                                    },
                                                    new QualityProfileQualityItem
                                                    {
                                                        Quality = Quality.WEBRip720p,
                                                        MinSize = 2,
                                                        MaxSize = 20,
                                                    }
                                                }
                                            }
                                        }
                                    })
                                    .Build();

            _episodes = Builder<Rom>.CreateListOfSize(10)
                .All()
                .With(s => s.PlatformNumber = 1)
                .With(s => s.Runtime = 30)
                .BuildList();

            _parseResultMultiSet = new RemoteEpisode
                                    {
                                        Game = _series,
                                        Release = new ReleaseInfo(),
                                        ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Roms = Builder<Rom>.CreateListOfSize(6)
                                            .All()
                                            .With(s => s.PlatformNumber = 1)
                                            .With(s => s.Runtime = 30)
                                            .BuildList()
                                    };

            _parseResultMulti = new RemoteEpisode
                                    {
                                        Game = _series,
                                        Release = new ReleaseInfo(),
                                        ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Roms = Builder<Rom>.CreateListOfSize(2)
                                            .All()
                                            .With(s => s.PlatformNumber = 1)
                                            .With(s => s.Runtime = 30)
                                            .BuildList()
                                    };

            _parseResultSingle = new RemoteEpisode
                                    {
                                        Game = _series,
                                        Release = new ReleaseInfo(),
                                        ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Roms = new List<Rom>
                                        {
                                            Builder<Rom>.CreateNew()
                                                .With(s => s.PlatformNumber = 1)
                                                .With(s => s.EpisodeNumber = 1)
                                                .With(s => s.Runtime = 30)
                                                .Build()
                                        }
                                    };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<IRomService>().Setup(
                s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_episodes);
        }

        private void WithSize(int? minSize, int? maxSize)
        {
            _series.QualityProfile.Value.Items[0].MinSize = minSize;
            _series.QualityProfile.Value.Items[0].MaxSize = maxSize;
        }

        [TestCase(30, 50, false)]
        [TestCase(30, 250, true)]
        [TestCase(30, 500, false)]
        [TestCase(60, 100, false)]
        [TestCase(60, 500, true)]
        [TestCase(60, 1000, false)]
        public void single_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultSingle.Roms.First().Id = 5;
            _parseResultSingle.Roms.First().Runtime = runtime;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 250, true)]
        [TestCase(30, 1000, false)]
        [TestCase(60, 250, true)]
        [TestCase(60, 2000, false)]
        public void should_return_expected_result_for_first_episode_of_season(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultSingle.Roms.First().Id = _episodes.First().Id;
            _parseResultSingle.Roms.First().Runtime = _episodes.First().Runtime;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 250, true)]
        [TestCase(30, 1000, false)]
        [TestCase(60, 250, true)]
        [TestCase(60, 2000, false)]
        public void should_return_expected_result_for_last_episode_of_season(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultSingle.Roms.First().Id = _episodes.Last().Id;
            _parseResultSingle.Roms.First().Runtime = _episodes.First().Runtime;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 50 * 2, false)]
        [TestCase(30, 250 * 2, true)]
        [TestCase(30, 500 * 2, false)]
        [TestCase(60, 100 * 2, false)]
        [TestCase(60, 500 * 2, true)]
        [TestCase(60, 1000 * 2, false)]
        public void multi_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultMulti.Game = _series;
            _parseResultMulti.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultMulti.Roms.ForEach(e => e.Runtime = runtime);

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 50 * 6, false)]
        [TestCase(30, 250 * 6, true)]
        [TestCase(30, 500 * 6, false)]
        [TestCase(60, 100 * 6, false)]
        [TestCase(60, 500 * 6, true)]
        [TestCase(60, 1000 * 6, false)]
        public void multiset_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultMultiSet.Game = _series;
            _parseResultMultiSet.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultMultiSet.Roms.ForEach(e => e.Runtime = runtime);

            Subject.IsSatisfiedBy(_parseResultMultiSet, new()).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            WithSize(10, 20);

            _series.Runtime = 30;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = 0;
            _parseResultSingle.Roms.First().Runtime = 30;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_30_minute()
        {
            WithSize(2, null);

            _series.Runtime = 30;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = 18457280000;
            _parseResultSingle.Roms.First().Runtime = 30;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_60_minute()
        {
            WithSize(2, null);

            _series.Runtime = 60;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = 36857280000;
            _parseResultSingle.Roms.First().Runtime = 60;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_treat_daily_series_as_single_episode()
        {
            _series.Runtime = 60;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Game.SeriesType = GameTypes.Daily;
            _parseResultSingle.Release.Size = 300.Megabytes();
            _parseResultSingle.Roms.First().Runtime = 60;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_use_grouped_quality_limits()
        {
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.WEBDL720p);

            _series.Runtime = 30;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Game.SeriesType = GameTypes.Daily;
            _parseResultSingle.Release.Size = 500.Megabytes();
            _parseResultSingle.Roms.First().Runtime = 30;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_RAWHD()
        {
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.RAWHD);

            _series.Runtime = 45;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Game.SeriesType = GameTypes.Daily;
            _parseResultSingle.Release.Size = 8000.Megabytes();
            _parseResultSingle.Roms.First().Runtime = 30;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_special()
        {
            _parseResultSingle.ParsedRomInfo.Special = true;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_single_episode_is_not_from_first_season()
        {
            _series.Runtime = 0;
            _parseResultSingle.Game = _series;
            _parseResultSingle.Roms.First().Id = 5;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Roms.First().PlatformNumber = 2;
            _parseResultSingle.Roms.First().Runtime = 0;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_single_episode_aired_more_than_24_hours_after_first_aired_episode()
        {
            _series.Runtime = 0;

            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Roms.First().Id = 5;
            _parseResultSingle.Roms.First().PlatformNumber = 1;
            _parseResultSingle.Roms.First().EpisodeNumber = 2;
            _parseResultSingle.Roms.First().AirDateUtc = _episodes.First().AirDateUtc.Value.AddDays(7);
            _parseResultSingle.Roms.First().Runtime = 0;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_true_if_series_runtime_is_zero_and_single_episode_aired_less_than_24_hours_after_first_aired_episode()
        {
            _series.Runtime = 0;

            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Roms.First().Id = 5;
            _parseResultSingle.Roms.First().PlatformNumber = 1;
            _parseResultSingle.Roms.First().EpisodeNumber = 2;
            _parseResultSingle.Roms.First().AirDateUtc = _episodes.First().AirDateUtc.Value.AddHours(1);
            _parseResultSingle.Roms.First().Runtime = 0;

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(true);
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_multi_episode_is_not_from_first_season()
        {
            _series.Runtime = 0;
            _parseResultMulti.Game = _series;
            _parseResultMulti.Release.Size = 200.Megabytes();
            _parseResultMulti.Roms.ForEach(e =>
            {
                e.PlatformNumber = 2;
                e.Runtime = 0;
            });

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_multi_episode_aired_more_than_24_hours_after_first_aired_episode()
        {
            var airDateUtc = _episodes.First().AirDateUtc.Value.AddDays(7);

            _series.Runtime = 0;

            _parseResultMulti.Game = _series;
            _parseResultMulti.Release.Size = 200.Megabytes();
            _parseResultMulti.Roms.ForEach(e =>
            {
                e.PlatformNumber = 1;
                e.AirDateUtc = airDateUtc;
                e.Runtime = 0;
            });

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_true_if_series_runtime_is_zero_and_multi_episode_aired_less_than_24_hours_after_first_aired_episode()
        {
            var airDateUtc = _episodes.First().AirDateUtc.Value.AddHours(1);

            _series.Runtime = 0;

            _parseResultMulti.Game = _series;
            _parseResultMulti.Release.Size = 200.Megabytes();
            _parseResultMulti.Roms.ForEach(e =>
            {
                e.PlatformNumber = 1;
                e.AirDateUtc = airDateUtc;
                e.Runtime = 0;
            });

            Subject.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().Be(true);
        }

        [Test]
        public void should_use_series_runtime_if_episode_runtime_is_not_set()
        {
            var airDateUtc = _episodes.First().AirDateUtc.Value.AddHours(1);

            _series.Runtime = 30;

            _parseResultSingle.Game = _series;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Roms.ForEach(e =>
            {
                e.PlatformNumber = 1;
                e.AirDateUtc = airDateUtc;
                e.Runtime = 0;
            });

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().Be(true);
        }
    }
}
