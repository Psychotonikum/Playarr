using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class AbsoluteRomNumberSpecificationFixture : CoreTest<AbsoluteRomNumberSpecification>
    {
        private Game _series;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Anime)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .With(e => e.AirDateUtc = DateTime.UtcNow)
                                           .Build()
                                           .ToList();

            _localRom = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Roms = roms,
                                    Game = _series
                                };

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresAbsoluteRomNumber())
                  .Returns(true);
        }

        [Test]
        public void should_reject_when_absolute_episode_number_is_null()
        {
            _localRom.Roms.First().AbsoluteEpisodeNumber = null;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_did_not_air_recently_but_absolute_episode_number_is_null()
        {
            _localRom.Roms.First().AirDateUtc = DateTime.UtcNow.AddDays(-7);
            _localRom.Roms.First().AbsoluteEpisodeNumber = null;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_absolute_episode_number_is_not_required()
        {
            _localRom.Roms.First().AbsoluteEpisodeNumber = null;

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresAbsoluteRomNumber())
                  .Returns(false);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
