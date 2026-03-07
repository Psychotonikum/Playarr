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
    public class GameTitleFirstCharacterFixture : CoreTest<FileNameBuilder>
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

        [TestCase("The Mist", "M\\The Mist")]
        [TestCase("A", "A\\A")]
        [TestCase("30 Rock", "3\\30 Rock")]
        [TestCase("The '80s Greatest", "8\\The '80s Greatest")]
        [TestCase("좀비버스", "좀\\좀비버스")]
        [TestCase("¡Mucha Lucha!", "M\\¡Mucha Lucha!")]
        [TestCase(".hack", "H\\hack")]
        [TestCase("Ütopya", "U\\Ütopya")]
        [TestCase("Æon Flux", "A\\Æon Flux")]

        public void should_get_expected_folder_name_back(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.GameFolderFormat = "{Game TitleFirstCharacter}\\{Game Title}";

            Subject.GetGameFolder(_series).Should().Be(expected);
        }

        [Test]
        public void should_be_able_to_use_lower_case_first_character()
        {
            _series.Title = "Westworld";
            _namingConfig.GameFolderFormat = "{game titlefirstcharacter}\\{game title}";

            Subject.GetGameFolder(_series).Should().Be("w\\westworld");
        }
    }
}
