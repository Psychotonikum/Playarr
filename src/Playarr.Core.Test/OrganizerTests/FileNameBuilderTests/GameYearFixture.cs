using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class SeriesYearFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("The Mist", 2017, "2017\\The Mist")]
        [TestCase("A", 2021, "2021\\A")]
        [TestCase("30 Rock", 2006, "2006\\30 Rock")]
        public void should_get_expected_folder_name_back(string title, int year, string expected)
        {
            _series.Title = title;
            _series.Year = year;
            _namingConfig.GameFolderFormat = "{Game Year}\\{Game Title}";

            Subject.GetGameFolder(_series).Should().Be(expected);
        }
    }
}
