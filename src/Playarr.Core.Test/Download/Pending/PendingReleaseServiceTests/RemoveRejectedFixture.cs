using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Download.Pending;
using Playarr.Core.Indexers;
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
    public class RemoveRejectedFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Game _series;
        private Rom _episode;
        private QualityProfile _profile;
        private ReleaseInfo _release;
        private ParsedRomInfo _parsedRomInfo;
        private RemoteEpisode _remoteRom;

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

            _series.QualityProfile = new LazyLoaded<QualityProfile>(_profile);

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedRomInfo = Builder<ParsedRomInfo>.CreateNew().Build();
            _parsedRomInfo.Quality = new QualityModel(Quality.HDTV720p);

            _remoteRom = new RemoteEpisode();
            _remoteRom.Roms = new List<Rom> { _episode };
            _remoteRom.Game = _series;
            _remoteRom.ParsedRomInfo = _parsedRomInfo;
            _remoteRom.Release = _release;

            _temporarilyRejected = new DownloadDecision(_remoteRom, new DownloadRejection(DownloadRejectionReason.MinimumAgeDelay, "Temp Rejected", RejectionType.Temporary));

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<PendingRelease>());

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

        private void GivenHeldRelease(string title, string indexer, DateTime publishDate)
        {
            var release = _release.JsonClone();
            release.Indexer = indexer;
            release.PublishDate = publishDate;

            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.SeriesId = _series.Id)
                                                   .With(h => h.Title = title)
                                                   .With(h => h.Release = release)
                                                   .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                                                   .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(heldReleases);
        }

        private void InitializeReleases()
        {
            Subject.Handle(new ApplicationStartedEvent());
        }

        [Test]
        public void should_remove_if_it_is_the_same_release_from_the_same_indexer()
        {
            GivenHeldRelease(_release.Title, _release.Indexer, _release.PublishDate);

            InitializeReleases();
            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

            VerifyDelete();
        }

        [Test]
        public void should_not_remove_if_title_is_different()
        {
            GivenHeldRelease(_release.Title + "-RP", _release.Indexer, _release.PublishDate);

            InitializeReleases();
            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

            VerifyNoDelete();
        }

        [Test]
        public void should_not_remove_if_indexer_is_different()
        {
            GivenHeldRelease(_release.Title, "AnotherIndexer", _release.PublishDate);

            InitializeReleases();
            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

            VerifyNoDelete();
        }

        [Test]
        public void should_not_remove_if_publish_date_is_different()
        {
            GivenHeldRelease(_release.Title, _release.Indexer, _release.PublishDate.AddHours(1));

            InitializeReleases();
            Subject.Handle(new RssSyncCompleteEvent(new ProcessedDecisions(new List<DownloadDecision>(),
                                                                           new List<DownloadDecision>(),
                                                                           new List<DownloadDecision> { _temporarilyRejected })));

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
