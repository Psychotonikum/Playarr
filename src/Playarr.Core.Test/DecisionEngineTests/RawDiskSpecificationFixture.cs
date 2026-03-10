using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class RawDiskSpecificationFixture : CoreTest<RawDiskSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteRom
            {
                Release = new ReleaseInfo
                          {
                              Title = "Game.title.s01e01",
                              DownloadProtocol = DownloadProtocol.Torrent
                          }
            };
        }

        private void WithContainer(string container)
        {
            _remoteRom.Release.Container = container;
        }

        [Test]
        public void should_return_true_if_no_container_specified_and_does_not_match_disc_release_pattern()
        {
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_mkv()
        {
            WithContainer("MKV");
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_vob()
        {
            WithContainer("VOB");
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_iso()
        {
            WithContainer("ISO");
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_m2ts()
        {
            WithContainer("M2TS");
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_compare_case_insensitive()
        {
            WithContainer("vob");
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [TestCase("Game Title S02 Disc 1 1080i Blu-ray DTS-HD MA 2.0 AVC-TrollHD")]
        [TestCase("Game Title S03 Disc 1 1080p Blu-ray LPCM 2.0 AVC-TrollHD")]
        [TestCase("GAME TITLE S02 1080P FULL BLURAY AVC DTS-HD MA 5 1")]
        [TestCase("Game.Title.S06.2016.DISC.3.BluRay.1080p.AVC.Atmos.TrueHD7.1-MTeam")]
        [TestCase("Game Title S05 Disc 1 BluRay 1080p AVC Atmos TrueHD 7 1-MTeam")]
        [TestCase("Game Title S05 Disc 1 BluRay 1080p AVC Atmos TrueHD 7 1-MTeam")]
        [TestCase("Someone.the.Entertainer.Presents.S01.NTSC.3xDVD9.MPEG-2.DD2.0")]
        [TestCase("Game.Title.S00.The.Christmas.Special.2011.PAL.DVD5.DD2.0")]
        [TestCase("Game.of.Desire.2000.S1_D01.NTSC.DVD5")]
        public void should_return_false_if_matches_disc_format(string title)
        {
            _remoteRom.Release.Title = title;
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [TestCase("Game Title EP50 USLT NTSC DVDRemux DD2.0")]
        [TestCase("Game.Title.S01.NTSC.DVDRip.DD2.0.x264-PLAiD")]
        public void should_return_true_if_dvdrip(string title)
        {
            _remoteRom.Release.Title = title;
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }
    }
}
