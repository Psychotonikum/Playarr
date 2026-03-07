using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.History;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Download.FailedDownloadServiceTests
{
    [TestFixture]
    public class ProcessFixture : CoreTest<FailedDownloadService>
    {
        private TrackedDownload _trackedDownload;
        private List<EpisodeHistory> _grabHistory;

        [SetUp]
        public void Setup()
        {
            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            _grabHistory = Builder<EpisodeHistory>.CreateListOfSize(2).BuildList();

            var remoteRom = new RemoteEpisode
            {
                Game = new Game(),
                Roms = new List<Rom> { new Rom { Id = 1 } }
            };

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadState.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteEpisode = remoteRom)
                    .Build();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Find(_trackedDownload.DownloadItem.DownloadId, EpisodeHistoryEventType.Grabbed))
                  .Returns(_grabHistory);
        }

        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.Find(_trackedDownload.DownloadItem.DownloadId, EpisodeHistoryEventType.Grabbed))
                .Returns(new List<EpisodeHistory>());
        }

        [Test]
        public void should_not_fail_if_matching_history_is_not_found()
        {
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            AssertDownloadNotFailed();
        }

        [Test]
        public void should_warn_if_matching_history_is_not_found()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_warn_if_matching_history_is_not_found_and_not_failed()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
        }

        private void AssertDownloadNotFailed()
        {
            Mocker.GetMock<IEventAggregator>()
               .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Never());

            _trackedDownload.State.Should().NotBe(TrackedDownloadState.Failed);
        }

        private void AssertDownloadFailed()
        {
            Mocker.GetMock<IEventAggregator>()
            .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadState.Failed);
        }
    }
}
