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
    public class AbsoluteEpisodeFormatFixture : CoreTest<FileNameBuilder>
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
                    .With(s => s.SeriesType = GameTypes.Anime)
                    .With(s => s.Title = "Anime Game")
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
        public void should_use_standard_format_if_absolute_format_requires_absolute_episode_number_and_it_is_missing()
        {
            _episode.AbsoluteEpisodeNumber = null;
            _namingConfig.StandardEpisodeFormat = "{Game Title} S{platform:00}E{rom:00}";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} {absolute:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("Anime Game S15E06");
        }

        [Test]
        public void should_use_absolute_format_if_absolute_format_requires_absolute_episode_number_and_it_is_available()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} S{platform:00}E{rom:00}";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} {absolute:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("Anime Game 100 [PlayarrTest]");
        }

        [Test]
        public void should_use_absolute_format_if_absolute_format_does_not_require_absolute_episode_number_and_it_is_not_available()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} S{platform:00}E{rom:00}";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} S{platform:00}E{rom:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be("Anime Game S15E06 [PlayarrTest]");
        }

        [Test]
        public void should_use_standard_format_without_absolute_numbering_if_absolute_format_requires_absolute_episode_number_and_it_is_missing()
        {
            _episode.AbsoluteEpisodeNumber = null;
            _namingConfig.StandardEpisodeFormat = "{Game Title} S{platform:00}E{rom:00} - {absolute:00}";
            _namingConfig.AnimeEpisodeFormat = "{Game Title} {absolute:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                .Should().Be("Anime Game S15E06");
        }
    }
}
