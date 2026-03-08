using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class RomTitleSpecificationFixture : CoreTest<RomTitleSpecification>
    {
        private Game _series;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Standard)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
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
                  .Setup(s => s.RequiresRomTitle(_series, roms))
                  .Returns(true);
        }

        [Test]
        public void should_reject_when_title_is_null()
        {
            _localRom.Roms.First().Title = null;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_when_title_is_TBA()
        {
            _localRom.Roms.First().Title = "TBA";

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_file_is_in_series_folder()
        {
            _localRom.ExistingFile = true;
            _localRom.Roms.First().Title = "TBA";

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_did_not_air_recently_but_title_is_TBA()
        {
            _localRom.Roms.First().AirDateUtc = DateTime.UtcNow.AddDays(-7);
            _localRom.Roms.First().Title = "TBA";

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_episode_title_is_not_required()
        {
            _localRom.Roms.First().Title = "TBA";

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.RequiresRomTitle(_series, _localRom.Roms))
                  .Returns(false);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_episode_title_is_never_required()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.RomTitleRequired)
                  .Returns(RomTitleRequiredType.Never);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_title_is_required_for_bulk_season_releases_and_not_bulk_season()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.RomTitleRequired)
                  .Returns(RomTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Rom>.CreateListOfSize(5).BuildList());

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_title_is_required_for_bulk_season_releases()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.RomTitleRequired)
                  .Returns(RomTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Rom>.CreateListOfSize(5)
                                           .All()
                                           .With(e => e.AirDateUtc == _localRom.Roms.First().AirDateUtc)
                                           .BuildList());

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_episode_title_is_required_for_bulk_season_releases_and_it_is_missing()
        {
            _localRom.Roms.First().Title = "TBA";

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.RomTitleRequired)
                  .Returns(RomTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Rom>.CreateListOfSize(5)
                                           .All()
                                           .With(e => e.AirDateUtc = _localRom.Roms.First().AirDateUtc)
                                           .BuildList());

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_if_episode_title_is_required_for_bulk_season_releases_and_some_episodes_do_not_have_air_date()
        {
            _localRom.Roms.First().Title = "TBA";

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.RomTitleRequired)
                  .Returns(RomTitleRequiredType.BulkSeasonReleases);

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Builder<Rom>.CreateListOfSize(5)
                                           .All()
                                           .With(e => e.Title  = "TBA")
                                           .With(e => e.AirDateUtc = null)
                                           .TheFirst(1)
                                           .With(e => e.AirDateUtc = _localRom.Roms.First().AirDateUtc)
                                           .BuildList());

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }
    }
}
