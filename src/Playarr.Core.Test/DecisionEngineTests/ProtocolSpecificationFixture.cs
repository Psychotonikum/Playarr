using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ProtocolSpecificationFixture : CoreTest<ProtocolSpecification>
    {
        private RemoteEpisode _remoteRom;
        private DelayProfile _delayProfile;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteEpisode();
            _remoteRom.Release = new ReleaseInfo();
            _remoteRom.Game = new Game();

            _delayProfile = new DelayProfile();

            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_delayProfile);
        }

        private void GivenProtocol(DownloadProtocol downloadProtocol)
        {
            _remoteRom.Release.DownloadProtocol = downloadProtocol;
        }

        [Test]
        public void should_be_true_if_usenet_and_usenet_is_enabled()
        {
            GivenProtocol(DownloadProtocol.Usenet);
            _delayProfile.EnableUsenet = true;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().Be(true);
        }

        [Test]
        public void should_be_true_if_torrent_and_torrent_is_enabled()
        {
            GivenProtocol(DownloadProtocol.Torrent);
            _delayProfile.EnableTorrent = true;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().Be(true);
        }

        [Test]
        public void should_be_false_if_usenet_and_usenet_is_disabled()
        {
            GivenProtocol(DownloadProtocol.Usenet);
            _delayProfile.EnableUsenet = false;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().Be(false);
        }

        [Test]
        public void should_be_false_if_torrent_and_torrent_is_disabled()
        {
            GivenProtocol(DownloadProtocol.Torrent);
            _delayProfile.EnableTorrent = false;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().Be(false);
        }
    }
}
