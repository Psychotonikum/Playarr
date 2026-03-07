using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetGameFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("30 Rock", "{Game Title}", "30 Rock")]
        [TestCase("30 Rock", "{Game.Title}", "30.Rock")]
        [TestCase("24/7 Road to the NHL Winter Classic", "{Game Title}", "24+7 Road to the NHL Winter Classic")]
        [TestCase("Venture Bros.", "{Game.Title}", "Venture.Bros")]
        [TestCase(".hack", "{Game.Title}", "hack")]
        [TestCase("30 Rock", ".{Game.Title}.", "30.Rock")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string gameTitle, string format, string expected)
        {
            _namingConfig.GameFolderFormat = format;

            var game = new Game { Title = gameTitle };

            Subject.GetGameFolder(game).Should().Be(expected);
        }
    }
}
