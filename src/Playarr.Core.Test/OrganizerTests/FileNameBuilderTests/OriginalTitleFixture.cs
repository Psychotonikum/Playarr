using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.MediaFiles;
using Playarr.Core.Organizer;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class OriginalTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _episode;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "My Game")
                    .Build();

            _episode = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _romFile = new RomFile { Id = 5, Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "PlayarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_include_original_title_if_not_current_file_name()
        {
            _romFile.SceneName = "my.game.s15e06";
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - S15E06 - City Sushi [my.game.s15e06]");
        }

        [Test]
        public void should_include_current_filename_if_not_renaming_files()
        {
            _romFile.SceneName = "my.game.s15e06";
            _namingConfig.RenameEpisodes = false;

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("my.game.s15e06");
        }

        [Test]
        public void should_include_current_filename_if_not_including_season_and_episode_tokens_for_standard_series()
        {
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - S15E06 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_include_current_filename_if_not_including_air_date_token_for_daily_series()
        {
            _series.SeriesType = GameTypes.Standard;
            _episode.AirDate = "2022-04-28";
            _romFile.RelativePath = "My Game - 2022-04-28 - City Sushi";
            _namingConfig.DailyEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - 2022-04-28 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_include_current_filename_if_not_including_absolute_episode_number_token_for_anime_series()
        {
            _series.SeriesType = GameTypes.Standard;
            _episode.AbsoluteEpisodeNumber = 123;
            _romFile.RelativePath = "My Game - 123 - City Sushi";
            _namingConfig.AnimeEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - 123 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_not_include_current_filename_if_including_season_and_episode_tokens_for_standard_series()
        {
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - S15E06");
        }

        [Test]
        public void should_not_include_current_filename_if_including_air_date_token_for_daily_series()
        {
            _series.SeriesType = GameTypes.Standard;
            _episode.AirDate = "2022-04-28";
            _romFile.RelativePath = "My Game - 2022-04-28 - City Sushi";
            _namingConfig.DailyEpisodeFormat = "{Game Title} - {Air-Date} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - 2022-04-28");
        }

        [Test]
        public void should_not_include_current_filename_if_including_absolute_episode_number_token_for_anime_series()
        {
            _series.SeriesType = GameTypes.Standard;
            _episode.AbsoluteEpisodeNumber = 123;
            _romFile.RelativePath = "My Game - 123 - City Sushi";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} - {absolute:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - 123");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_season_and_episode_tokens_for_standard_series()
        {
            _romFile.Id = 0;
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - S15E06 [My Game - S15E06 - City Sushi]");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_air_date_token_for_daily_series()
        {
            _series.SeriesType = GameTypes.Standard;
            _episode.AirDate = "2022-04-28";
            _romFile.Id = 0;
            _romFile.RelativePath = "My Game - 2022-04-28 - City Sushi";
            _namingConfig.DailyEpisodeFormat = "{Game Title} - {Air-Date} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - 2022-04-28 [My Game - 2022-04-28 - City Sushi]");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_absolute_episode_number_token_for_anime_series()
        {
            _series.SeriesType = GameTypes.Standard;
            _episode.AbsoluteEpisodeNumber = 123;
            _romFile.Id = 0;
            _romFile.RelativePath = "My Game - 123 - City Sushi";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} - {absolute:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("My Game - 123 [My Game - 123 - City Sushi]");
        }
    }
}
