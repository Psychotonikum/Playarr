using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Core.Datastore;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles
{
    public class UpgradeMediaFileServiceFixture : CoreTest<UpgradeMediaFileService>
    {
        private RomFile _romFile;
        private LocalEpisode _localRom;

        [SetUp]
        public void Setup()
        {
            _localRom = new LocalEpisode();
            _localRom.Game = new Game
                                   {
                                       Path = @"C:\Test\TV\Game".AsOsAgnostic()
                                   };

            _romFile = Builder<RomFile>
                .CreateNew()
                .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(Directory.GetParent(_localRom.Game.Path).FullName))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.GetParentFolder(It.IsAny<string>()))
                  .Returns<string>(c => Path.GetDirectoryName(c));
        }

        private void GivenSingleEpisodeWithSingleRomFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 1,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e01.avi",
                                                                                })
                                                     .Build()
                                                     .ToList();
        }

        private void GivenMultipleEpisodesWithSingleRomFile()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 1,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e01.avi",
                                                                                })
                                                     .Build()
                                                     .ToList();
        }

        private void GivenMultipleEpisodesWithMultipleRomFiles()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 1,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e01.avi",
                                                                                })
                                                     .TheNext(1)
                                                     .With(e => e.RomFile = new RomFile
                                                                                {
                                                                                    Id = 2,
                                                                                    RelativePath = @"Platform 01\30.rock.s01e02.avi",
                                                                                })
                                                     .Build()
                                                     .ToList();
        }

        [Test]
        public void should_delete_single_episode_file_once()
        {
            GivenSingleEpisodeWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_the_same_episode_file_only_once()
        {
            GivenMultipleEpisodesWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_multiple_different_episode_files()
        {
            GivenMultipleEpisodesWithMultipleRomFiles();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void should_delete_episode_file_from_database()
        {
            GivenSingleEpisodeWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<RomFile>(), DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_delete_existing_file_fromdb_if_file_doesnt_exist()
        {
            GivenSingleEpisodeWithSingleRomFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localRom.Roms.Single().RomFile, DeleteMediaFileReason.Upgrade), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_try_to_recyclebin_existing_file_if_file_doesnt_exist()
        {
            GivenSingleEpisodeWithSingleRomFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_old_episode_file_in_oldFiles()
        {
            GivenSingleEpisodeWithSingleRomFile();

            Subject.UpgradeRomFile(_romFile, _localRom).OldFiles.Count.Should().Be(1);
        }

        [Test]
        public void should_return_old_episode_files_in_oldFiles()
        {
            GivenMultipleEpisodesWithMultipleRomFiles();

            Subject.UpgradeRomFile(_romFile, _localRom).OldFiles.Count.Should().Be(2);
        }

        [Test]
        public void should_throw_if_there_are_existing_episode_files_and_the_root_folder_is_missing()
        {
            GivenSingleEpisodeWithSingleRomFile();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(Directory.GetParent(_localRom.Game.Path).FullName))
                  .Returns(false);

            Assert.Throws<RootFolderNotFoundException>(() => Subject.UpgradeRomFile(_romFile, _localRom));

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localRom.Roms.Single().RomFile, DeleteMediaFileReason.Upgrade), Times.Never());
        }

        [Test]
        public void should_import_if_existing_file_doesnt_exist_in_db()
        {
            _localRom.Roms = Builder<Rom>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.RomFile = new LazyLoaded<RomFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.UpgradeRomFile(_romFile, _localRom);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localRom.Roms.Single().RomFile, It.IsAny<DeleteMediaFileReason>()), Times.Never());
        }
    }
}
