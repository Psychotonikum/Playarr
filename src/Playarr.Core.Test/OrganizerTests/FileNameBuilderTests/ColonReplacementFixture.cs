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
    public class ColonReplacementFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _episode1;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "CSI: Vegas")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "What Happens in Vegas")
                            .With(e => e.PlatformNumber = 1)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "PlayarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_replace_colon_followed_by_space_with_space_dash_space_by_default()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title}";

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                   .Should().Be("CSI - Vegas");
        }

        [TestCase("CSI: Vegas", ColonReplacementFormat.Smart, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Dash, "CSI- Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Delete, "CSI Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDash, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDashSpace, "CSI - Vegas")]
        public void should_replace_colon_followed_by_space_with_expected_result(string seriesName, ColonReplacementFormat replacementFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardEpisodeFormat = "{Game Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be(expected);
        }

        [TestCase("Game:Title", ColonReplacementFormat.Smart, "Game-Title")]
        [TestCase("Game:Title", ColonReplacementFormat.Dash, "Game-Title")]
        [TestCase("Game:Title", ColonReplacementFormat.Delete, "GameTitle")]
        [TestCase("Game:Title", ColonReplacementFormat.SpaceDash, "Game -Title")]
        [TestCase("Game:Title", ColonReplacementFormat.SpaceDashSpace, "Game - Title")]
        public void should_replace_colon_with_expected_result(string seriesName, ColonReplacementFormat replacementFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardEpisodeFormat = "{Game Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be(expected);
        }

        [TestCase("Game: Title", ColonReplacementFormat.Custom, "\ua789", "Game\ua789 Title")]
        [TestCase("Game: Title", ColonReplacementFormat.Custom, "∶", "Game∶ Title")]
        public void should_replace_colon_with_custom_format(string seriesName, ColonReplacementFormat replacementFormat, string customFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardEpisodeFormat = "{Game Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;
            _namingConfig.CustomColonReplacementFormat = customFormat;

            Subject.BuildFileName(new List<Rom> { _episode1 }, _series, _romFile)
                .Should().Be(expected);
        }
    }
}
