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

    public class MultiEpisodeFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _episode1;
        private Rom _episode2;
        private Rom _episode3;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episode2 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 7)
                            .With(e => e.AbsoluteEpisodeNumber = 101)
                            .Build();

            _episode3 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 8)
                            .With(e => e.AbsoluteEpisodeNumber = 102)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "PlayarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _romFile.Quality.Revision.Version = 2;
        }

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_format_extend_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = 0;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                .Should().Be("South Park - S15E06-07 - City Sushi");
        }

        [Test]
        public void should_format_duplicate_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Duplicate;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                .Should().Be("South Park - S15E06 - S15E07 - City Sushi");
        }

        [Test]
        public void should_format_repeat_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Repeat;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                .Should().Be("South Park - S15E06E07 - City Sushi");
        }

        [Test]
        public void should_format_scene_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Scene;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                .Should().Be("South Park - S15E06-E07 - City Sushi");
        }

        [Test]
        public void should_get_proper_filename_when_multi_episode_is_duplicated_and_bracket_follows_pattern()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Game Title} - S{platform:00}E{rom:00} - ({Quality Title}, {MediaInfo Full}, {Release Group}) - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Duplicate;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - S15E07 - (Unknown, , PlayarrTest) - City Sushi");
        }

        [Test]
        public void should_format_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Range;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2, _episode3 }, _series, _romFile)
                .Should().Be("South Park - S15E06-08 - City Sushi");
        }

        [Test]
        public void should_format_single_episode_with_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Range;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_single_anime_episode_with_range_multi_episode_properly()
        {
            _series.SeriesType = GameTypes.Standard;
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Range;
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_default_to_dash_when_serparator_is_not_set()
        {
            _series.SeriesType = GameTypes.Standard;
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Duplicate;
            _namingConfig.StandardEpisodeFormat = "{Game Title} - {platform}x{rom:00} - {Rom Title} - {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                   .Should().Be("South Park - 15x06 - 15x07 - City Sushi - Unknown");
        }

        [Test]
        public void should_format_prefixed_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2, _episode3 }, _series, _romFile)
                .Should().Be("South Park - S15E06-E08 - City Sushi");
        }

        [Test]
        public void should_format_prefixed_range_multi_episode_anime_properly()
        {
            _series.SeriesType = GameTypes.Standard;
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange;
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2, _episode3 }, _series, _romFile)
                   .Should().Be("South Park - S15E06-E08 - City Sushi");
        }

        [Test]
        public void should_format_single_episode_with_prefixed_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_single_anime_episode_with_prefixed_range_multi_episode_properly()
        {
            _series.SeriesType = GameTypes.Standard;
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange;
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_prefixed_range_multi_episode_using_episode_separator()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} - {platform:0}x{rom:00} - {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2, _episode3 }, _series, _romFile)
                .Should().Be("South Park - 15x06-x08 - City Sushi");
        }

        [Test]
        public void should_format_range_multi_episode_wrapped_in_brackets()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} (S{platform:00}E{rom:00}) {Rom Title}";
            _namingConfig.MultiEpisodeStyle = MultiEpisodeStyle.Range;

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2, _episode3 }, _series, _romFile)
                .Should().Be("South Park (S15E06-08) City Sushi");
        }
    }
}
