using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests
{
    [TestFixture]
    public class GetPlatformFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("Venture Bros.", 1, "{Game.Title}.{platform:00}", "Venture.Bros.01")]
        [TestCase("Venture Bros.", 1, "{Game Title} Platform {platform:00}", "Venture Bros. Platform 01")]
        [TestCase("Game Title?", 1, "{Game Title} Platform {platform:00}", "Game Title! Platform 01")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string gameTitle, int platformNumber, string format, string expected)
        {
            _namingConfig.PlatformFolderFormat = format;

            var game = new Game { Title = gameTitle };

            Subject.GetPlatformFolder(game, platformNumber, _namingConfig).Should().Be(expected);
        }
    }
}
