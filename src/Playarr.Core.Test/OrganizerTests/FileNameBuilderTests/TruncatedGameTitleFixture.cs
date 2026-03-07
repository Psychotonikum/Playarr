using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.Organizer;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TruncatedGameTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "Game Title")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.MultiEpisodeStyle = 0;
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

        [TestCase("{Game Title:16}", "The Fantastic...")]
        [TestCase("{Game TitleThe:17}", "Fantastic Life...")]
        [TestCase("{Game CleanTitle:-13}", "...Mr. Sisko")]
        public void should_truncate_series_title(string format, string expected)
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";
            _namingConfig.GameFolderFormat = format;

            var result = Subject.GetGameFolder(_series, _namingConfig);
            result.Should().Be(expected);
        }
    }
}
