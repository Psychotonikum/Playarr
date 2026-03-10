using System.Collections.Generic;
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
    public class RepackSpecificationFixture : CoreTest<RepackSpecification>
    {
        private ParsedRomInfo _parsedRomInfo;
        private List<Rom> _episodes;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _parsedRomInfo = Builder<ParsedRomInfo>.CreateNew()
                                                           .With(p => p.Quality = new QualityModel(Quality.SDTV,
                                                               new Revision(2, 0, false)))
                                                           .With(p => p.ReleaseGroup = "Playarr")
                                                           .Build();

            _episodes = Builder<Rom>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.EpisodeFileId = 0)
                                        .BuildList();
        }

        [Test]
        public void should_return_true_if_it_is_not_a_repack()
        {
            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_there_are_is_no_episode_file()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_a_different_quality()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.DVD))
                                                                .With(e => e.ReleaseGroup = "Playarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_existing_file()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Playarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_a_different_file()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "NotPlayarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_existing_file_is_unknown()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_release_is_unknown()
        {
            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _parsedRomInfo.ReleaseGroup = null;

            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Playarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_true_when_repacks_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.DoNotPrefer);

            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Playarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_repack_but_auto_download_repacks_is_true()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Playarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_repack_but_auto_download_repacks_is_false()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.DoNotUpgrade);

            _parsedRomInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().RomFile = Builder<RomFile>.CreateNew()
                                                                .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                                .With(e => e.ReleaseGroup = "Playarr")
                                                                .Build();

            var remoteRom = Builder<RemoteRom>.CreateNew()
                                                      .With(e => e.ParsedRomInfo = _parsedRomInfo)
                                                      .With(e => e.Roms = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
