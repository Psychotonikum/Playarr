using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class RequiresRomTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _episode;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _episode = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_return_false_when_episode_title_is_not_part_of_the_pattern()
        {
            _namingConfig.StandardEpisodeFormat = "{Game Title} S{platform:00}E{rom:00}";
            Subject.RequiresRomTitle(_series, new List<Rom> { _episode }).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_renaming_episodes_is_off()
        {
            _namingConfig.RenameEpisodes = false;
            Subject.RequiresRomTitle(_series, new List<Rom> { _episode }).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_episode_title_is_part_of_the_pattern()
        {
            Subject.RequiresRomTitle(_series, new List<Rom> { _episode }).Should().BeTrue();
        }
    }
}
