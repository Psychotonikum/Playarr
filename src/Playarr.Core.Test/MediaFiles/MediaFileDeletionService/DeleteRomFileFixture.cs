using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Exceptions;
using Playarr.Core.MediaFiles;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class DeleteRomFileFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private const string ROOT_FOLDER = @"C:\Test\TV";
        private Game _series;
        private RomFile _romFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = Path.Combine(ROOT_FOLDER, "Game Title"))
                                     .Build();

            _romFile = Builder<RomFile>.CreateNew()
                                               .With(f => f.RelativePath = "Game Title - S01E01")
                                               .With(f => f.Path = Path.Combine(_series.Path, "Game Title - S01E01"))
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_series.Path))
                  .Returns(ROOT_FOLDER);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_romFile.Path))
                  .Returns(_series.Path);
        }

        private void GivenRootFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(ROOT_FOLDER))
                  .Returns(true);
        }

        private void GivenRootFolderHasFolders()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(ROOT_FOLDER))
                  .Returns(new[] { _series.Path });
        }

        private void GivenGameFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_series.Path))
                  .Returns(true);
        }

        [Test]
        public void should_throw_if_root_folder_does_not_exist()
        {
            Assert.Throws<PlayarrClientException>(() => Subject.DeleteRomFile(_series, _romFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_should_throw_if_root_folder_is_empty()
        {
            GivenRootFolderExists();

            Assert.Throws<PlayarrClientException>(() => Subject.DeleteRomFile(_series, _romFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_delete_from_db_if_series_folder_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();

            Subject.DeleteRomFile(_series, _romFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_romFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_romFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_db_if_episode_file_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenGameFolderExists();

            Subject.DeleteRomFile(_series, _romFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_romFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_romFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_disk_and_db_if_episode_file_exists()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenGameFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_romFile.Path))
                  .Returns(true);

            Subject.DeleteRomFile(_series, _romFile);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_romFile.Path, "Game Title"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_romFile, DeleteMediaFileReason.Manual), Times.Once());
        }

        [Test]
        public void should_handle_error_deleting_episode_file()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenGameFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_romFile.Path))
                  .Returns(true);

            Mocker.GetMock<IRecycleBinProvider>()
                  .Setup(s => s.DeleteFile(_romFile.Path, "Game Title"))
                  .Throws(new IOException());

            Assert.Throws<PlayarrClientException>(() => Subject.DeleteRomFile(_series, _romFile));

            ExceptionVerification.IgnoreWarns();
            ExceptionVerification.IgnoreErrors();
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_romFile.Path, "Game Title"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_romFile, DeleteMediaFileReason.Manual), Times.Never());
        }
    }
}
