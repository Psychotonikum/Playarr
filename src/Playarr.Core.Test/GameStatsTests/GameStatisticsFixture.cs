using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Qualities;
using Playarr.Core.GameStats;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.GameStatsTests
{
    [TestFixture]
    public class SeriesStatisticsFixture : DbTest<SeriesStatisticsRepository, Game>
    {
        private Game _series;
        private Rom _episode;
        private RomFile _romFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            _series.Id = Db.Insert(_series).Id;

            _episode = Builder<Rom>.CreateNew()
                                          .With(e => e.EpisodeFileId = 0)
                                          .With(e => e.Monitored = false)
                                          .With(e => e.SeriesId = _series.Id)
                                          .With(e => e.AirDateUtc = DateTime.Today.AddDays(5))
                                          .BuildNew();

            _romFile = Builder<RomFile>.CreateNew()
                                           .With(e => e.SeriesId = _series.Id)
                                           .With(e => e.Quality = new QualityModel(Quality.HDTV720p))
                                           .With(e => e.Languages = new List<Language> { Language.English })
                                           .BuildNew();
        }

        private void GivenEpisodeWithFile()
        {
            _episode.EpisodeFileId = 1;
        }

        private void GivenOldEpisode()
        {
            _episode.AirDateUtc = DateTime.Now.AddSeconds(-10);
        }

        private void GivenMonitoredEpisode()
        {
            _episode.Monitored = true;
        }

        private void GivenEpisode()
        {
            Db.Insert(_episode);
        }

        private void GivenRomFile()
        {
            Db.Insert(_romFile);
        }

        [Test]
        public void should_get_stats_for_series()
        {
            GivenMonitoredEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().BeCloseTo(_episode.AirDateUtc.Value, TimeSpan.FromMilliseconds(1000));
            stats.First().PreviousAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_have_next_airing_for_episode_with_file()
        {
            GivenEpisodeWithFile();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_have_previous_airing_for_old_episode_without_file_monitored()
        {
            GivenMonitoredEpisode();
            GivenOldEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().BeCloseTo(_episode.AirDateUtc.Value, TimeSpan.FromMilliseconds(1000));
        }

        [Test]
        public void should_not_have_previous_airing_for_old_episode_without_file_unmonitored()
        {
            GivenOldEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_include_unmonitored_episode_in_episode_count()
        {
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().EpisodeCount.Should().Be(0);
        }

        [Test]
        public void should_include_unmonitored_episode_with_file_in_episode_count()
        {
            GivenEpisodeWithFile();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().EpisodeCount.Should().Be(1);
        }

        [Test]
        public void should_have_size_on_disk_of_zero_when_no_episode_file()
        {
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(0);
        }

        [Test]
        public void should_have_size_on_disk_when_episode_file_exists()
        {
            GivenEpisodeWithFile();
            GivenEpisode();
            GivenRomFile();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_romFile.Size);
        }

        [Test]
        public void should_not_duplicate_size_for_multi_episode_files()
        {
            GivenEpisodeWithFile();
            GivenEpisode();
            GivenRomFile();

            var episode2 = _episode.JsonClone();

            episode2.Id = 0;
            episode2.EpisodeNumber += 1;

            Db.Insert(episode2);

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_romFile.Size);
        }
    }
}
