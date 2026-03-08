using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Download.Pending;
using Playarr.Core.Lifecycle;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemoveGrabbedFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Game _series;
        private Rom _episode;
        private QualityProfile _profile;
        private ReleaseInfo _release;
        private ParsedRomInfo _parsedRomInfo;
        private RemoteEpisode _remoteRom;
        private List<PendingRelease> _heldReleases;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .Build();

            _episode = Builder<Rom>.CreateNew()
                                       .Build();

            _profile = new QualityProfile
                       {
                           Name = "Test",
                           Cutoff = Quality.HDTV720p.Id,
                           Items = new List<QualityProfileQualityItem>
                                   {
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.HDTV720p },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.WEBDL720p },
                                       new QualityProfileQualityItem { Allowed = true, Quality = Quality.Bluray720p }
                                   },
                       };

            _series.QualityProfile = _profile;

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedRomInfo = Builder<ParsedRomInfo>.CreateNew()
                                                           .With(h => h.Quality = new QualityModel(Quality.HDTV720p))
                                                           .With(h => h.AirDate = null)
                                                           .Build();

            _remoteRom = new RemoteEpisode();
            _remoteRom.Roms = new List<Rom> { _episode };
            _remoteRom.Game = _series;
            _remoteRom.ParsedRomInfo = _parsedRomInfo;
            _remoteRom.Release = _release;

            _temporarilyRejected = new DownloadDecision(_remoteRom, new DownloadRejection(DownloadRejectionReason.MinimumAgeDelay, "Temp Rejected", RejectionType.Temporary));

            _heldReleases = new List<PendingRelease>();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_heldReleases);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.AllByGameId(It.IsAny<int>()))
                  .Returns<int>(i => _heldReleases.Where(v => v.GameId == i).ToList());

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(_series);

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetSeries(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Game> { _series });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<Game>()))
                  .Returns(new RemoteEpisode { Roms = new List<Rom> { _episode } });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<ParsedRomInfo>(), _series, true, null))
                  .Returns(new List<Rom> { _episode });

            Mocker.GetMock<IPrioritizeDownloadDecision>()
                  .Setup(s => s.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns((List<DownloadDecision> d) => d);
        }

        private void GivenHeldRelease(QualityModel quality)
        {
            var parsedRomInfo = _parsedRomInfo.JsonClone();
            parsedRomInfo.Quality = quality;

            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.GameId = _series.Id)
                                                   .With(h => h.Release = _release.JsonClone())
                                                   .With(h => h.ParsedRomInfo = parsedRomInfo)
                                                   .Build();

            _heldReleases.AddRange(heldReleases);
        }

        private void InitializeReleases()
        {
            Subject.Handle(new ApplicationStartedEvent());
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_same()
        {
            GivenHeldRelease(_parsedRomInfo.Quality);

            InitializeReleases();
            Subject.Handle(new EpisodeGrabbedEvent(_remoteRom));

            VerifyDelete();
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_higher()
        {
            GivenHeldRelease(new QualityModel(Quality.SDTV));

            InitializeReleases();
            Subject.Handle(new EpisodeGrabbedEvent(_remoteRom));

            VerifyDelete();
        }

        [Test]
        public void should_not_delete_if_the_grabbed_quality_is_the_lower()
        {
            GivenHeldRelease(new QualityModel(Quality.Bluray720p));

            InitializeReleases();
            Subject.Handle(new EpisodeGrabbedEvent(_remoteRom));

            VerifyNoDelete();
        }

        private void VerifyDelete()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Delete(It.IsAny<PendingRelease>()), Times.Once());
        }

        private void VerifyNoDelete()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Delete(It.IsAny<PendingRelease>()), Times.Never());
        }
    }
}
