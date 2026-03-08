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
    public class CleanTitleTheFixture : CoreTest<FileNameBuilder>
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
                    .Build();

            _episode = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "PlayarrTest" };

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

        [TestCase("The Mist", "Mist, The")]
        [TestCase("A Place to Call Home", "Place to Call Home, A")]
        [TestCase("An Adventure in Space and Time", "Adventure in Space and Time, An")]
        [TestCase("The Flash (2010)", "Flash, The 2010")]
        [TestCase("A League Of Their Own (AU)", "League Of Their Own, A AU")]
        [TestCase("The Fixer (ZH) (2015)", "Fixer, The ZH 2015")]
        [TestCase("The Sixth Sense 2 (Thai)", "Sixth Sense 2, The Thai")]
        [TestCase("The Amazing Race (Latin America)", "Amazing Race, The Latin America")]
        [TestCase("The Rat Pack (A&E)", "Rat Pack, The AandE")]
        [TestCase("The Climax: I (Almost) Got Away With It (2016)", "Climax I Almost Got Away With It, The 2016")]
        public void should_get_expected_title_back(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Game CleanTitleThe}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be(expected);
        }

        [TestCase("A")]
        [TestCase("Anne")]
        [TestCase("Theodore")]
        [TestCase("3%")]
        public void should_not_change_title(string title)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Game CleanTitleThe}";

            Subject.BuildFileName(new List<Rom> { _episode }, _series, _romFile)
                   .Should().Be(title);
        }
    }
}
