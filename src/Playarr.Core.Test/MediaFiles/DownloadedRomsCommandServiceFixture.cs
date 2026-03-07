using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Commands;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles
{
    [TestFixture]
    public class DownloadedEpisodesCommandServiceFixture : CoreTest<DownloadedEpisodesCommandService>
    {
        private string _downloadFolder = "c:\\drop_other\\Show.S01E01\\".AsOsAgnostic();
        private string _downloadFile = "c:\\drop_other\\Show.S01E01.mkv".AsOsAgnostic();

        private TrackedDownload _trackedDownload;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Setup(v => v.ProcessRootFolder(It.IsAny<DirectoryInfo>()))
                .Returns(new List<ImportResult>());

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Game>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>());

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                .With(v => v.DownloadId = "sab1")
                .With(v => v.Status = DownloadItemStatus.Downloading)
                .Build();

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                .With(v => v.Game = new Game())
                .Build();

            _trackedDownload = new TrackedDownload
                    {
                        DownloadItem = downloadItem,
                        RemoteEpisode = remoteRom,
                        State = TrackedDownloadState.Downloading
                    };
        }

        private void GivenExistingFolder(string path)
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>()))
                    .Returns(true);
        }

        private void GivenExistingFile(string path)
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(It.IsAny<string>()))
                    .Returns(true);
        }

        private void GivenValidQueueItem()
        {
            Mocker.GetMock<ITrackedDownloadService>()
                  .Setup(s => s.Find("sab1"))
                  .Returns(_trackedDownload);
        }

        [Test]
        public void should_skip_import_if_dronefactory_doesnt_exist()
        {
            Assert.Throws<ArgumentException>(() => Subject.Execute(new DownloadedEpisodesScanCommand()));

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessRootFolder(It.IsAny<DirectoryInfo>()), Times.Never());
        }

        [Test]
        public void should_process_folder_if_downloadclientid_is_not_specified()
        {
            GivenExistingFolder(_downloadFolder);

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder });

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessPath(It.IsAny<string>(), ImportMode.Auto, null, null), Times.Once());
        }

        [Test]
        public void should_process_file_if_downloadclientid_is_not_specified()
        {
            GivenExistingFile(_downloadFile);

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFile });

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessPath(It.IsAny<string>(), ImportMode.Auto, null, null), Times.Once());
        }

        [Test]
        public void should_process_folder_with_downloadclientitem_if_available()
        {
            GivenExistingFolder(_downloadFolder);
            GivenValidQueueItem();

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder, DownloadClientId = "sab1" });

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessPath(_downloadFolder, ImportMode.Auto, _trackedDownload.RemoteEpisode.Game, _trackedDownload.DownloadItem), Times.Once());
        }

        [Test]
        public void should_process_folder_without_downloadclientitem_if_not_available()
        {
            GivenExistingFolder(_downloadFolder);

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder, DownloadClientId = "sab1" });

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessPath(_downloadFolder, ImportMode.Auto, null, null), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_warn_if_neither_folder_or_file_exists()
        {
            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFolder });

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessPath(It.IsAny<string>(), ImportMode.Auto, null, null), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_override_import_mode()
        {
            GivenExistingFile(_downloadFile);

            Subject.Execute(new DownloadedEpisodesScanCommand() { Path = _downloadFile, ImportMode = ImportMode.Copy });

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessPath(It.IsAny<string>(), ImportMode.Copy, null, null), Times.Once());
        }
    }
}
