using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Extras;
using Playarr.Core.Extras.Files;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Extras
{
    [TestFixture]
    public class ExtraServiceFixture : CoreTest<ExtraService>
    {
        private Game _series;
        private RomFile _romFile;
        private LocalEpisode _localRom;

        private string _seriesFolder;
        private string _episodeFolder;

        private Mock<IManageExtraFiles> _subtitleService;
        private Mock<IManageExtraFiles> _otherExtraService;

        [SetUp]
        public void Setup()
        {
            _seriesFolder = @"C:\Test\TV\Game Title".AsOsAgnostic();
            _episodeFolder = @"C:\Test\Unsorted TV\Game.Title.S01".AsOsAgnostic();

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = _seriesFolder)
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .Build()
                                           .ToList();

            _romFile = Builder<RomFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Platform 1", "Game Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Platform 1\Game Title - S01E01.mkv".AsOsAgnostic())
                                               .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Game = _series)
                                                 .With(l => l.Roms = roms)
                                                 .With(l => l.Path = Path.Combine(_episodeFolder, "Game.Title.S01E01.mkv").AsOsAgnostic())
                                                 .Build();

            _subtitleService = new Mock<IManageExtraFiles>();
            _subtitleService.SetupGet(s => s.Order).Returns(0);
            _subtitleService.Setup(s => s.CanImportFile(It.IsAny<LocalEpisode>(), It.IsAny<RomFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(false);
            _subtitleService.Setup(s => s.CanImportFile(It.IsAny<LocalEpisode>(), It.IsAny<RomFile>(), It.IsAny<string>(), ".srt", It.IsAny<bool>()))
                .Returns(true);

            _otherExtraService = new Mock<IManageExtraFiles>();
            _otherExtraService.SetupGet(s => s.Order).Returns(1);
            _otherExtraService.Setup(s => s.CanImportFile(It.IsAny<LocalEpisode>(), It.IsAny<RomFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(true);

            Mocker.SetConstant<IEnumerable<IManageExtraFiles>>(new[]
            {
                _subtitleService.Object,
                _otherExtraService.Object
            });

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);

            WithExistingFolder(_series.Path);
            WithExistingFile(_romFile.Path);
            WithExistingFile(_localRom.Path);

            Mocker.GetMock<IConfigService>().Setup(v => v.ImportExtraFiles).Returns(true);
            Mocker.GetMock<IConfigService>().Setup(v => v.ExtraFileExtensions).Returns("nfo,srt");
        }

        private void WithExistingFolder(string path, bool exists = true)
        {
            var dir = Path.GetDirectoryName(path);

            if (exists && dir.IsNotNullOrWhiteSpace())
            {
                WithExistingFolder(dir);
            }

            Mocker.GetMock<IDiskProvider>().Setup(v => v.FolderExists(path)).Returns(exists);
        }

        private void WithExistingFile(string path, bool exists = true, int size = 1000)
        {
            var dir = Path.GetDirectoryName(path);

            if (exists && dir.IsNotNullOrWhiteSpace())
            {
                WithExistingFolder(dir);
            }

            Mocker.GetMock<IDiskProvider>().Setup(v => v.FileExists(path)).Returns(exists);
            Mocker.GetMock<IDiskProvider>().Setup(v => v.GetFileSize(path)).Returns(size);
        }

        private void WithExistingFiles(List<string> files)
        {
            foreach (var file in files)
            {
                WithExistingFile(file);
            }

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(_episodeFolder, It.IsAny<bool>()))
                  .Returns(files.ToArray());
        }

        [Test]
        public void should_not_pass_file_if_import_disabled()
        {
            Mocker.GetMock<IConfigService>().Setup(v => v.ImportExtraFiles).Returns(false);

            var nfofile = Path.Combine(_episodeFolder, "Game.Title.S01E01.nfo").AsOsAgnostic();

            var files = new List<string>
            {
                _localRom.Path,
                nfofile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localRom, _romFile, true);

            _subtitleService.Verify(v => v.CanImportFile(_localRom, _romFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
            _otherExtraService.Verify(v => v.CanImportFile(_localRom, _romFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        [TestCase("Game Title - S01E01.sub")]
        [TestCase("Game Title - S01E01.ass")]
        public void should_not_pass_unwanted_file(string filePath)
        {
            Mocker.GetMock<IConfigService>().Setup(v => v.ImportExtraFiles).Returns(false);

            var nfofile = Path.Combine(_episodeFolder, filePath).AsOsAgnostic();

            var files = new List<string>
            {
                _localRom.Path,
                nfofile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localRom, _romFile, true);

            _subtitleService.Verify(v => v.CanImportFile(_localRom, _romFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
            _otherExtraService.Verify(v => v.CanImportFile(_localRom, _romFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_pass_subtitle_file_to_subtitle_service()
        {
            var subtitleFile = Path.Combine(_episodeFolder, "Game.Title.S01E01.en.srt").AsOsAgnostic();

            var files = new List<string>
            {
                _localRom.Path,
                subtitleFile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localRom, _romFile, true);

            _subtitleService.Verify(v => v.ImportFiles(_localRom, _romFile, new List<string> { subtitleFile }, true), Times.Once());
            _otherExtraService.Verify(v => v.ImportFiles(_localRom, _romFile, new List<string> { subtitleFile }, true), Times.Never());
        }

        [Test]
        public void should_pass_nfo_file_to_other_service()
        {
            var nfofile = Path.Combine(_episodeFolder, "Game.Title.S01E01.nfo").AsOsAgnostic();

            var files = new List<string>
            {
                _localRom.Path,
                nfofile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localRom, _romFile, true);

            _subtitleService.Verify(v => v.ImportFiles(_localRom, _romFile, new List<string> { nfofile }, true), Times.Never());
            _otherExtraService.Verify(v => v.ImportFiles(_localRom, _romFile, new List<string> { nfofile }, true), Times.Once());
        }

        [Test]
        public void should_search_subtitles_when_importing_from_job_folder()
        {
            _localRom.FolderRomInfo = new ParsedRomInfo();

            var subtitleFile = Path.Combine(_episodeFolder, "Game.Title.S01E01.en.srt").AsOsAgnostic();

            var files = new List<string>
            {
                _localRom.Path,
                subtitleFile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localRom, _romFile, true);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, true), Times.Once);
            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, false), Times.Never);
        }

        [Test]
        public void should_not_search_subtitles_when_not_importing_from_job_folder()
        {
            _localRom.FolderRomInfo = null;

            var subtitleFile = Path.Combine(_episodeFolder, "Game.Title.S01E01.en.srt").AsOsAgnostic();

            var files = new List<string>
            {
                _localRom.Path,
                subtitleFile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localRom, _romFile, true);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, true), Times.Never);
            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, false), Times.Once);
        }
    }
}
