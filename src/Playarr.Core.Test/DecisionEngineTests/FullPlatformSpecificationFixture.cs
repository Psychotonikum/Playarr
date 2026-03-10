using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class FullSeasonSpecificationFixture : CoreTest<FullSeasonSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            var show = Builder<Game>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteRom = new RemoteRom
            {
                ParsedRomInfo = new ParsedRomInfo
                {
                    FullSeason = true
                },
                Roms = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.GameId = show.Id)
                                           .BuildList(),
                Game = show,
                Release = new ReleaseInfo
                {
                    Title = "Game.Title.S01.720p.BluRay.X264-RlsGrp"
                }
            };
        }

        [Test]
        public void should_return_true_if_is_not_a_full_season()
        {
            _remoteRom.ParsedRomInfo.FullSeason = false;
            _remoteRom.Roms.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_all_episodes_have_aired()
        {
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_all_episodes_will_have_aired_in_the_next_24_hours()
        {
            _remoteRom.Roms.Last().AirDateUtc = DateTime.UtcNow.AddHours(23);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_episode_has_not_aired()
        {
            _remoteRom.Roms.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_an_episode_does_not_have_an_air_date()
        {
            _remoteRom.Roms.Last().AirDateUtc = null;
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
