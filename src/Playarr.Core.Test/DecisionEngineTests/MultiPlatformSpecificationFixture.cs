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
    public class MultiSeasonSpecificationFixture : CoreTest<MultiSeasonSpecification>
    {
        private RemoteEpisode _remoteRom;

        [SetUp]
        public void Setup()
        {
            var game = Builder<Game>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteRom = new RemoteEpisode
            {
                ParsedRomInfo = new ParsedRomInfo
                {
                    FullSeason = true,
                    IsMultiSeason = true
                },
                Roms = Builder<Rom>.CreateListOfSize(3)
                                           .All()
                                           .With(s => s.SeriesId = game.Id)
                                           .BuildList(),
                Game = game,
                Release = new ReleaseInfo
                {
                    Title = "Game.Title.S01-05.720p.BluRay.X264-RlsGrp"
                }
            };
        }

        [Test]
        public void should_return_true_if_is_not_a_multi_season_release()
        {
            _remoteRom.ParsedRomInfo.IsMultiSeason = false;
            _remoteRom.Roms.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_is_a_multi_season_release()
        {
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
