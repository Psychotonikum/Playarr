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

    public class MultiRomTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _episode1;
        private Rom _episode2;
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
                            .With(e => e.Title = "Rom Title")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episode2 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Rom Title")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 7)
                            .With(e => e.AbsoluteEpisodeNumber = 101)
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

        [TestCase("Rom Title (1)", "Rom Title (2)")]
        [TestCase("Rom Title Part 1", "Rom Title Part 2")]
        [TestCase("Rom Title", "Rom Title: Part 2")]
        public void should_replace_Series_space_Title(string firstTitle, string secondTitle)
        {
            _episode1.Title = firstTitle;
            _episode2.Title = secondTitle;

            _namingConfig.StandardEpisodeFormat = "{Rom Title} {Quality Full}";

            Subject.BuildFileName(new List<Rom> { _episode1, _episode2 }, _series, _romFile)
                   .Should().Be("Rom Title HDTV-720p");
        }
    }
}
