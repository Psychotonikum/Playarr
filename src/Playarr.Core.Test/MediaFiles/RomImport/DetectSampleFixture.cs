using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class DetectSampleFixture : CoreTest<DetectSample>
    {
        private Game _series;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Standard)
                                     .With(s => s.Runtime = 30)
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .Build()
                                           .ToList();

            _localRom = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Roms = roms,
                                    Game = _series,
                                    Quality = new QualityModel(Quality.HDTV720p),
                                };
        }

        private void GivenRuntime(int seconds)
        {
            var runtime = new TimeSpan(0, 0, seconds);

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns(runtime);

            _localRom.MediaInfo = Builder<MediaInfoModel>.CreateNew().With(m => m.RunTime = runtime).Build();
        }

        [Test]
        public void should_return_false_if_season_zero()
        {
            _localRom.Roms[0].PlatformNumber = 0;

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_for_flv()
        {
            _localRom.Path = @"C:\Test\some.show.s01e01.flv";

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_for_strm()
        {
            _localRom.Path = @"C:\Test\some.show.s01e01.strm";

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime()
        {
            GivenRuntime(120);

            Subject.IsSample(_localRom.Game,
                             _localRom.Path,
                             _localRom.IsSpecial);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_runtime_is_less_than_minimum()
        {
            GivenRuntime(60);

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.Sample);
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_minimum()
        {
            GivenRuntime(600);

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_webisode_minimum()
        {
            _series.Runtime = 6;
            GivenRuntime(299);

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_anime_short_minimum()
        {
            _series.Runtime = 2;
            GivenRuntime(60);

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_true_if_runtime_less_than_anime_short_minimum()
        {
            _series.Runtime = 2;
            GivenRuntime(10);

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.Sample);
        }

        [Test]
        public void should_return_indeterminate_if_mediainfo_result_is_null()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns((TimeSpan?)null);

            Subject.IsSample(_localRom.Game,
                             _localRom.Path,
                             _localRom.IsSpecial).Should().Be(DetectSampleResult.Indeterminate);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_treat_daily_episode_a_special()
        {
            GivenRuntime(600);
            _series.SeriesType = GameTypes.Daily;
            _localRom.Roms[0].PlatformNumber = 0;

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_return_false_for_anime_special()
        {
            _series.SeriesType = GameTypes.Anime;
            _localRom.Roms[0].PlatformNumber = 0;

            Subject.IsSample(_localRom.Game,
                _localRom.Path,
                _localRom.IsSpecial).Should().Be(DetectSampleResult.NotSample);
        }

        [Test]
        public void should_use_runtime_from_media_info()
        {
            GivenRuntime(120);

            _localRom.Game.Runtime = 30;
            _localRom.Roms.First().Runtime = 30;

            Subject.IsSample(_localRom).Should().Be(DetectSampleResult.Sample);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime_from_episode_over_series()
        {
            GivenRuntime(120);

            _localRom.Game.Runtime = 5;
            _localRom.Roms.First().Runtime = 30;

            Subject.IsSample(_localRom).Should().Be(DetectSampleResult.Sample);
        }

        [Test]
        public void should_default_to_45_minutes_if_runtime_is_zero()
        {
            GivenRuntime(120);

            _localRom.Game.Runtime = 0;
            _localRom.Roms.First().Runtime = 0;

            Subject.IsSample(_localRom).Should().Be(DetectSampleResult.Sample);
        }
    }
}
