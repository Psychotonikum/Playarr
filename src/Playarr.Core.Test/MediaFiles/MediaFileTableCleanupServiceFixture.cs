using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.MediaFiles;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles
{
    public class MediaFileTableCleanupServiceFixture : CoreTest<MediaFileTableCleanupService>
    {
        private const string DELETED_PATH = "ANY FILE WITH THIS PATH IS CONSIDERED DELETED!";
        private List<Rom> _episodes;
        private Game _series;

        [SetUp]
        public void SetUp()
        {
            _episodes = Builder<Rom>.CreateListOfSize(10)
                  .Build()
                  .ToList();

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Game".AsOsAgnostic())
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(e => e.FileExists(It.Is<string>(c => !c.Contains(DELETED_PATH))))
                  .Returns(true);

            Mocker.GetMock<IRomService>()
                  .Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenRomFiles(IEnumerable<RomFile> romFiles)
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesBySeries(It.IsAny<int>()))
                  .Returns(romFiles.ToList());
        }

        private void GivenFilesAreNotAttachedToEpisode()
        {
            _episodes.ForEach(e => e.EpisodeFileId = 0);

            Mocker.GetMock<IRomService>()
                  .Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private List<string> FilesOnDisk(IEnumerable<RomFile> romFiles)
        {
            return romFiles.Select(e => Path.Combine(_series.Path, e.RelativePath)).ToList();
        }

        [Test]
        public void should_skip_files_that_exist_in_disk()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(10)
                .Build();

            GivenRomFiles(romFiles);

            Subject.Clean(_series, FilesOnDisk(romFiles));

            Mocker.GetMock<IRomService>().Verify(c => c.UpdateEpisode(It.IsAny<Rom>()), Times.Never());
        }

        [Test]
        public void should_delete_non_existent_files()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(10)
                .Random(2)
                .With(c => c.RelativePath = DELETED_PATH)
                .Build();

            GivenRomFiles(romFiles);

            Subject.Clean(_series, FilesOnDisk(romFiles.Where(e => e.RelativePath != DELETED_PATH)));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.Is<RomFile>(e => e.RelativePath == DELETED_PATH), DeleteMediaFileReason.MissingFromDisk), Times.Exactly(2));
        }

        [Test]
        public void should_delete_files_that_dont_belong_to_any_episodes()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.RelativePath = "ExistingPath")
                                .Build();

            GivenRomFiles(romFiles);
            GivenFilesAreNotAttachedToEpisode();

            Subject.Clean(_series, FilesOnDisk(romFiles));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.IsAny<RomFile>(), DeleteMediaFileReason.NoLinkedEpisodes), Times.Exactly(10));
        }

        [Test]
        public void should_unlink_episode_when_romFile_does_not_exist()
        {
            GivenRomFiles(new List<RomFile>());

            Subject.Clean(_series, new List<string>());

            Mocker.GetMock<IRomService>().Verify(c => c.UpdateEpisode(It.Is<Rom>(e => e.EpisodeFileId == 0)), Times.Exactly(10));
        }

        [Test]
        public void should_not_update_episode_when_romFile_exists()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.RelativePath = "ExistingPath")
                                .Build();

            GivenRomFiles(romFiles);

            Subject.Clean(_series, FilesOnDisk(romFiles));

            Mocker.GetMock<IRomService>().Verify(c => c.UpdateEpisode(It.IsAny<Rom>()), Times.Never());
        }
    }
}
