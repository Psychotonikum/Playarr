using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AnimeVersionUpgradeSpecificationFixture : CoreTest<AnimeVersionUpgradeSpecification>
    {
        private AnimeVersionUpgradeSpecification _subject;
        private RemoteRom _remoteRom;
        private RomFile _romFile;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.PreferAndUpgrade);

            Mocker.Resolve<UpgradableSpecification>();
            _subject = Mocker.Resolve<AnimeVersionUpgradeSpecification>();

            _romFile = new RomFile
                           {
                               Quality = new QualityModel(Quality.HDTV720p, new Revision()),
                               ReleaseGroup = "DRONE2"
                           };

            _remoteRom = new RemoteRom();
            _remoteRom.Game = new Game { SeriesType = GameTypes.Standard };
            _remoteRom.ParsedRomInfo = new ParsedRomInfo
                                               {
                                                   Quality = new QualityModel(Quality.HDTV720p, new Revision(2)),
                                                   ReleaseGroup = "DRONE"
                                               };

            _remoteRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                      .All()
                                                      .With(e => e.RomFile = _romFile)
                                                      .Build()
                                                      .ToList();
        }

        private void GivenStandardSeries()
        {
            _remoteRom.Game.SeriesType = GameTypes.Standard;
        }

        private void GivenNoVersionUpgrade()
        {
            _remoteRom.ParsedRomInfo.Quality.Revision = new Revision();
        }

        [Test]
        public void should_be_true_when_no_existing_file()
        {
            _remoteRom.Roms.First().EpisodeFileId = 0;

            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_series_is_not_anime()
        {
            GivenStandardSeries();
            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_is_not_a_version_upgrade_for_existing_file()
        {
            GivenNoVersionUpgrade();
            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_group_matches()
        {
            _romFile.ReleaseGroup = _remoteRom.ParsedRomInfo.ReleaseGroup;

            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_existing_file_doesnt_have_a_release_group()
        {
            _romFile.ReleaseGroup = string.Empty;
            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_should_be_false_when_release_doesnt_have_a_release_group()
        {
            _remoteRom.ParsedRomInfo.ReleaseGroup = string.Empty;
            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_when_release_group_does_not_match()
        {
            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_repacks_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            _subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }
    }
}
