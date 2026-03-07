using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class UpdateMediaInfoServiceFixture : CoreTest<UpdateMediaInfoService>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = new Game
                      {
                          Id = 1,
                          Path = @"C:\game".AsOsAgnostic()
                      };

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableMediaInfo)
                  .Returns(true);
        }

        private void GivenFileExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenSuccessfulScan()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(It.IsAny<string>()))
                  .Returns(new MediaInfoModel());
        }

        private void GivenFailedScan(string path)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(path))
                  .Returns((MediaInfoModel)null);
        }

        [Test]
        public void should_skip_up_to_date_media_info()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel { SchemaRevision = VideoFileInfoReader.CURRENT_MEDIA_INFO_SCHEMA_REVISION })
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(romFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_skip_not_yet_date_media_info()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel { SchemaRevision = VideoFileInfoReader.MINIMUM_MEDIA_INFO_SCHEMA_REVISION })
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(romFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_update_outdated_media_info()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel())
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(romFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(3));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Exactly(3));
        }

        [Test]
        public void should_ignore_missing_files()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.RelativePath = "media.mkv")
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(romFiles);

            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo("media.mkv"), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Never());
        }

        [Test]
        public void should_continue_after_failure()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.Path = null)
                   .With(v => v.RelativePath = "media.mkv")
                   .TheFirst(1)
                   .With(v => v.RelativePath = "media2.mkv")
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(romFiles);

            GivenFileExists();
            GivenSuccessfulScan();
            GivenFailedScan(Path.Combine(_series.Path, "media2.mkv"));

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(1));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Exactly(1));
        }

        [Test]
        public void should_not_update_files_if_media_info_disabled()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(2)
                .All()
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.RelativePath = "media2.mkv")
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                .Setup(v => v.GetFilesBySeries(1))
                .Returns(romFiles);

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.EnableMediaInfo)
                .Returns(false);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Never());
        }

        [Test]
        public void should_not_update_if_media_info_disabled()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .Build();

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.EnableMediaInfo)
                .Returns(false);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Update(romFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<RomFile>()), Times.Never());
        }

        [Test]
        public void should_update_media_info()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(v => v.Path = null)
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Update(romFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(romFile), Times.Once());
        }

        [Test]
        public void should_not_update_media_info_if_new_info_is_null()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenFailedScan(Path.Combine(_series.Path, "media.mkv"));

            Subject.Update(romFile, _series);

            romFile.MediaInfo.Should().NotBeNull();
        }

        [Test]
        public void should_not_save_episode_file_if_new_info_is_null()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenFailedScan(Path.Combine(_series.Path, "media.mkv"));

            Subject.Update(romFile, _series);

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(romFile), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_file_does_not_support_media_info()
        {
            var path = Path.Combine(_series.Path, "media.iso");

            var romFile = Builder<RomFile>.CreateNew()
                .With(v => v.Path = path)
                .Build();

            GivenFileExists();
            GivenFailedScan(path);

            Subject.Update(romFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(path), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(romFile), Times.Never());
        }
    }
}
