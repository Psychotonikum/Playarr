using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SameEpisodesSpecificationFixture : CoreTest<SameEpisodesSpecification>
    {
        private List<Rom> _episodes;

        [SetUp]
        public void Setup()
        {
            _episodes = Builder<Rom>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.EpisodeFileId = 1)
                                        .BuildList();
        }

        private void GivenEpisodesInFile(List<Rom> roms)
        {
            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesByFileId(It.IsAny<int>()))
                  .Returns(roms);
        }

        [Test]
        public void should_not_upgrade_when_new_release_contains_less_episodes()
        {
            GivenEpisodesInFile(_episodes);

            Subject.IsSatisfiedBy(new List<Rom> { _episodes.First() }).Should().BeFalse();
        }

        [Test]
        public void should_upgrade_when_new_release_contains_more_episodes()
        {
            GivenEpisodesInFile(new List<Rom> { _episodes.First() });

            Subject.IsSatisfiedBy(_episodes).Should().BeTrue();
        }

        [Test]
        public void should_upgrade_when_new_release_contains_the_same_episodes()
        {
            GivenEpisodesInFile(_episodes);

            Subject.IsSatisfiedBy(_episodes).Should().BeTrue();
        }

        [Test]
        public void should_upgrade_when_release_contains_the_same_episodes_as_multiple_files()
        {
            var roms = Builder<Rom>.CreateListOfSize(2)
                                           .BuildList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesByFileId(roms.First().EpisodeFileId))
                  .Returns(new List<Rom> { roms.First() });

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesByFileId(roms.Last().EpisodeFileId))
                  .Returns(new List<Rom> { roms.Last() });

            Subject.IsSatisfiedBy(roms).Should().BeTrue();
        }
    }
}
